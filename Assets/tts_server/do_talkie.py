import argparse
import json
import logging
import os
import platform
from datetime import datetime

import soundfile as sf
import torch

from cli.SparkTTS import SparkTTS


def parse_args():
    """Parse command-line arguments."""
    parser = argparse.ArgumentParser(description='Generate audio files.')

    parser.add_argument(
        '--model_dir',
        type=str,
        default='pretrained_models/Spark-TTS-0.5B',
        help='Path to the model directory',
    )
    parser.add_argument(
        '--save_dir',
        type=str,
        default='example/results',
        help='Directory to save generated audio files',
    )
    parser.add_argument(
        '--prompt_text', type=str, help='Transcript of prompt audio'
    )
    parser.add_argument(
        '--prompt_speech_path',
        type=str,
        help='Path to the prompt audio file',
    )
    parser.add_argument(
        '--talkies_file',
        type=str,
        help='Path to the prompt audio file',
        default='voicelines.json',
    )
    return parser.parse_args()


def run_tts(args):
    """Perform TTS inference and save the generated audio."""
    # Ensure the save directory exists
    os.makedirs(args.save_dir, exist_ok=True)

    # Initialize the model
    model = SparkTTS(args.model_dir, device)

    with open(args.talkies_file) as file:
        lines = json.loads(file.read())

    for uuid, line in lines.items():
        # Perform inference and save the output audio
        save_path = os.path.join(args.save_dir, f"{uuid}.wav")

        with torch.no_grad():
            wav = model.inference(
                line,
                args.prompt_speech_path,
                prompt_text=args.prompt_text,
            )
            sf.write(save_path, wav, samplerate=16000)

        logging.info(f'Audio saved at: {save_path}')


if __name__ == '__main__':
    args = parse_args()

    if platform.system() == 'Darwin' and torch.backends.mps.is_available():
        device = torch.device(f'mps')
        logging.info(f'Using MPS device: {device}')
    elif torch.cuda.is_available():
        device = torch.device(f'cuda')
        logging.info(f'Using CUDA device: {device}')
    else:
        device = torch.device('cpu')
        logging.info('GPU acceleration not available, using CPU')

    run_tts(args)
