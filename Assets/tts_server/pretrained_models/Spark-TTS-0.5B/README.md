---
license: cc-by-nc-sa-4.0
language:
  - en
  - zh
tags:
- text-to-speech
library_tag: spark-tts
---


<div align="center">
    <h1>
    Spark-TTS
    </h1>
    <p>
    Official model for <br>
    <b><em>Spark-TTS: An Efficient LLM-Based Text-to-Speech Model with Single-Stream Decoupled Speech Tokens</em></b>
    </p>
    <p>
    <img src="src/logo/SparkTTS.jpg" alt="Spark-TTS Logo" style="width: 200px; height: 200px;">
    </p>
</div>


## Spark-TTS 🔥

### 👉🏻 [Spark-TTS Demos](https://sparkaudio.github.io/spark-tts/) 👈🏻

### 👉🏻 [Github Repo](https://github.com/SparkAudio/Spark-TTS) 👈🏻

### 👉🏻 [Paper](https://arxiv.org/pdf/2503.01710) 👈🏻

### Overview

Spark-TTS is an advanced text-to-speech system that uses the power of large language models (LLM) for highly accurate and natural-sounding voice synthesis. It is designed to be efficient, flexible, and powerful for both research and production use.

### Key Features

- **Simplicity and Efficiency**: Built entirely on Qwen2.5, Spark-TTS eliminates the need for additional generation models like flow matching. Instead of relying on separate models to generate acoustic features, it directly reconstructs audio from the code predicted by the LLM. This approach streamlines the process, improving efficiency and reducing complexity.
- **High-Quality Voice Cloning**: Supports zero-shot voice cloning, which means it can replicate a speaker's voice even without specific training data for that voice. This is ideal for cross-lingual and code-switching scenarios, allowing for seamless transitions between languages and voices without requiring separate training for each one.
- **Bilingual Support**: Supports both Chinese and English, and is capable of zero-shot voice cloning for cross-lingual and code-switching scenarios, enabling the model to synthesize speech in multiple languages with high naturalness and accuracy.
- **Controllable Speech Generation**: Supports creating virtual speakers by adjusting parameters such as gender, pitch, and speaking rate.

---

<table align="center">
  <tr>
    <td align="center"><b>Inference Overview of Voice Cloning</b><br><img src="src/figures/infer_voice_cloning.png" width="80%" /></td>
  </tr>
  <tr>
    <td align="center"><b>Inference Overview of Controlled Generation</b><br><img src="src/figures/infer_control.png" width="80%" /></td>
  </tr>
</table>


## Install
**Clone and Install**

- Clone the repo
``` sh
git clone https://github.com/SparkAudio/Spark-TTS.git
cd Spark-TTS
```

- Install Conda: please see https://docs.conda.io/en/latest/miniconda.html
- Create Conda env:

``` sh
conda create -n sparktts -y python=3.12
conda activate sparktts
pip install -r requirements.txt
# If you are in mainland China, you can set the mirror as follows:
pip install -r requirements.txt -i https://mirrors.aliyun.com/pypi/simple/ --trusted-host=mirrors.aliyun.com
```

**Model Download**

Download via python:
```python
from huggingface_hub import snapshot_download

snapshot_download("SparkAudio/Spark-TTS-0.5B", local_dir="pretrained_models/Spark-TTS-0.5B")
```

Download via git clone:
```sh
mkdir -p pretrained_models

# Make sure you have git-lfs installed (https://git-lfs.com)
git lfs install

git clone https://huggingface.co/SparkAudio/Spark-TTS-0.5B pretrained_models/Spark-TTS-0.5B
```

**Basic Usage**

You can simply run the demo with the following commands:
``` sh
cd example
bash infer.sh
```

Alternatively, you can directly execute the following command in the command line to perform inference：

``` sh
python -m cli.inference \
    --text "text to synthesis." \
    --device 0 \
    --save_dir "path/to/save/audio" \
    --model_dir pretrained_models/Spark-TTS-0.5B \
    --prompt_text "transcript of the prompt audio" \
    --prompt_speech_path "path/to/prompt_audio"
```

**UI Usage**

You can start the UI interface by running `python webui.py`, which allows you to perform Voice Cloning and Voice Creation. Voice Cloning supports uploading reference audio or directly recording the audio.


| **Voice Cloning** | **Voice Creation** |
|:-------------------:|:-------------------:|
| ![Image 1](src/figures/gradio_TTS.png) | ![Image 2](src/figures/gradio_control.png) |


## To-Do List

- [x] Release the Spark-TTS paper.
- [ ] Release the training code.
- [ ] Release the training dataset, VoxBox.

## Citation

```
@misc{wang2025sparktts,
      title={Spark-TTS: An Efficient LLM-Based Text-to-Speech Model with Single-Stream Decoupled Speech Tokens}, 
      author={Xinsheng Wang and Mingqi Jiang and Ziyang Ma and Ziyu Zhang and Songxiang Liu and Linqin Li and Zheng Liang and Qixi Zheng and Rui Wang and Xiaoqin Feng and Weizhen Bian and Zhen Ye and Sitong Cheng and Ruibin Yuan and Zhixian Zhao and Xinfa Zhu and Jiahao Pan and Liumeng Xue and Pengcheng Zhu and Yunlin Chen and Zhifei Li and Xie Chen and Lei Xie and Yike Guo and Wei Xue},
      year={2025},
      eprint={2503.01710},
      archivePrefix={arXiv},
      primaryClass={cs.SD},
      url={https://arxiv.org/abs/2503.01710}, 
}
```


## ⚠ License Update

The model's license has been updated from Apache 2.0 to CC BY-NC-SA due to the licensing terms of some training data.

Key Changes:

- The model can only be used for non-commercial purposes.

- Any modifications or derivatives must also be released under CC BY-NC-SA 4.0.

- Proper attribution is required when using or modifying the model.

Please ensure compliance with the new license terms.


## ⚠️ Usage Disclaimer

This project provides a zero-shot voice cloning TTS model intended for academic research, educational purposes, and legitimate applications, such as personalized speech synthesis, assistive technologies, and linguistic research.

Please note:

- Do not use this model for unauthorized voice cloning, impersonation, fraud, scams, deepfakes, or any illegal activities.

- Ensure compliance with local laws and regulations when using this model and uphold ethical standards.

- The developers assume no liability for any misuse of this model.

We advocate for the responsible development and use of AI and encourage the community to uphold safety and ethical principles in AI research and applications. If you have any concerns regarding ethics or misuse, please contact us.