import os
import sys
import json
from faster_whisper import WhisperModel

# Tambahkan runtime CUDA ke PATH jika tersedia di sistem Windows pengguna
local_app_data = os.environ.get("LOCALAPPDATA", "")
cuda_candidates = [
    os.path.join(local_app_data, "Programs", "Ollama", "lib", "ollama", "cuda_v12"),
    os.path.join(local_app_data, "Programs", "Ollama", "lib", "ollama", "cuda_v11"),
]
for cuda_path in cuda_candidates:
    if os.path.exists(cuda_path):
        try:
            os.add_dll_directory(cuda_path)
        except AttributeError:
            pass
        os.environ["PATH"] = cuda_path + os.pathsep + os.environ["PATH"]
        break

def transcribe_and_split(audio_path, output_json=None):
    if not os.path.exists(audio_path):
        print(json.dumps({"error": f"Berkas audio tidak ditemukan: {audio_path}"}))
        sys.exit(1)

    model_path = os.environ.get("WHISPER_MODEL_DIR", os.path.join(os.path.dirname(__file__), "..", "models", "whisper_small"))
    if not os.path.exists(model_path):
        model_path = "small"

    try:
        model = WhisperModel(model_path, device="cuda", compute_type="float16")
    except Exception:
        model = WhisperModel(model_path, device="cpu", compute_type="int8")

    segments, info = model.transcribe(audio_path, language="id", word_timestamps=False)
    
    sentences = []
    for s in segments:
        txt = s.text.strip()
        if txt:
            sentences.append(txt)

    base_name = os.path.splitext(os.path.basename(audio_path))[0]
    # Bersihkan nama untuk judul topik
    clean_title = base_name.replace("_", " ").replace("-", " ").strip()

    result = {
        "success": True,
        "title": clean_title,
        "duration": round(info.duration, 2),
        "total_scenes": len(sentences),
        "sentences": sentences,
        "script_text": "\n".join(sentences)
    }

    if output_json:
        with open(output_json, "w", encoding="utf-8") as f:
            json.dump(result, f, indent=2, ensure_ascii=False)

    print("__JSON_TRANSCRIPT_START__")
    print(json.dumps(result, ensure_ascii=False))
    print("__JSON_TRANSCRIPT_END__")

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print(json.dumps({"error": "Penggunaan: python transcribe_and_split_vocal.py <path_audio> [output_json]"}))
        sys.exit(1)

    audio_file = sys.argv[1]
    out_file = sys.argv[2] if len(sys.argv) > 2 else None
    transcribe_and_split(audio_file, out_file)
