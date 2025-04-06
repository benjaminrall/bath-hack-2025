import os
import queue
import threading
import time

import soundfile as sf
import torch
from flask import Flask, json, jsonify, request

from cli.SparkTTS import SparkTTS

app = Flask(__name__)

app.config['REFERENCE_AUDIO_FOLDER'] = 'references'
app.config['GENERATED_AUDIO_FOLDER'] = 'generated'
app.config['VOICELINE_MAP'] = 'voicelines.json'
app.config['MODEL_DIR'] = 'pretrained_models/Spark-TTS-0.5B'
app.config['REFERENCE_SPEECH'] = 'To be or not to be, that is the question.'

task_queue: queue.Queue[str] = queue.Queue()

device = torch.device(
    # 'mps'
    # if torch.backends.mps.is_available()
    # else 'cuda'
    # if torch.cuda.is_available()
    # else 
    'cpu'
)

# Example worker function to process the queue
def task_worker():
    model = SparkTTS(app.config['MODEL_DIR'], device)

    with open(app.config['VOICELINE_MAP']) as file:
        lines = json.loads(file.read())

    while True:
        # Get the miiid from the queue
        filename = task_queue.get()

        # Locate the reference audio file
        reference_file = os.path.join(
            app.config['REFERENCE_AUDIO_FOLDER'], f'{filename}.mp3'
        )

        for uuid, line in lines.items():

            # Run inference to generate audio
            with torch.no_grad():
                wav = model.inference(
                    line,
                    reference_file,
                    app.config['REFERENCE_SPEECH'],
                )

                # Save dir = miiid/voicelineid.wav
                save_dir = os.path.join(
                    app.config['GENERATED_AUDIO_FOLDER'], filename,                 )

                if not os.path.exists(save_dir):
                    os.mkdir(save_dir)

                save_file = os.path.join(save_dir, f'{uuid}.wav')


                sf.write(save_file, wav, samplerate=16000)
                print("generated line:", line)


# Start the background thread
threading.Thread(target=task_worker, daemon=True).start()


@app.route('/generate_audio', methods=['POST'])
def upload_audio():
    data = request.get_json()

    if not data or 'miiid' not in data:
        return jsonify({'error': 'Missing "miiid" in request'}), 400

    # READ file `miiid.mp4` from REFERENCE_AUDIO_FOLDER
    miiid = data['miiid']

    task_queue.put(miiid)

    return (
        jsonify(
            {
                'message': 'HI',
            }
        ),
        200,
    )


if __name__ == '__main__':
    app.run(debug=True)
