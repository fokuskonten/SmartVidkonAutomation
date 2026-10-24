import os
import sys
import json
import glob

# ==============================================================================
# BOT ALIGNMENT TIMING AI LOKAL (FASTER-WHISPER OFFLINE ENGINE)
# FokusKonten Video Engine — v2.0 Word-Level Snapping Edition
# ==============================================================================
# Perubahan v2.0:
# - Mengekspor word_timing_master.json (stempel waktu tiap KATA per detik riil)
# - Hapus opening_offset 8.0 yang menyebabkan SRT misaligned 8 detik
# - Threshold align kalimat: 85% -> 95% agar ujung kalimat tidak bocor ke adegan berikutnya
# ==============================================================================

# Tambahkan runtime CUDA Ollama ke PATH jika tersedia di sistem Windows pengguna
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

def normalize(text):
    return "".join(c.lower() for c in text if c.isalnum())

def format_timestamp_srt(seconds):
    millis = int(round(seconds * 1000))
    hours = millis // 3600000
    millis %= 3600000
    minutes = millis // 60000
    millis %= 60000
    secs = millis // 1000
    millis %= 1000
    return f"{hours:02d}:{minutes:02d}:{secs:02d},{millis:03d}"

def run_alignment(project_dir):
    try:
        from faster_whisper import WhisperModel
    except ImportError:
        print("[ERROR] faster_whisper belum terpasang. Jalankan dengan: uv run --with faster-whisper python ...")
        sys.exit(1)

    print(f"\n=======================================================")
    print(f"  BOT DETEKSI TIMING GROUND-TRUTH (FASTER-WHISPER)")
    print(f"  Proyek: {project_dir}")
    print(f"  Versi : 2.0 Word-Level Snapping Edition")
    print(f"=======================================================\n")

    audio_dir = os.path.join(project_dir, "02_audio_master")
    prompts_dir = os.path.join(project_dir, "04_prompts")
    os.makedirs(prompts_dir, exist_ok=True)

    # 1. Cari berkas audio vokal (Prioritaskan berkas terbaru yang baru dikonversi / diunggah)
    audio_files = glob.glob(os.path.join(audio_dir, "*.mp3")) + glob.glob(os.path.join(audio_dir, "*.wav"))
    if not audio_files:
        print(f"[ERROR] Tidak ditemukan berkas audio di {audio_dir}")
        sys.exit(1)

    # Urutkan berdasarkan waktu modifikasi terbaru
    audio_files.sort(key=lambda f: os.path.getmtime(f), reverse=True)
    audio_path = audio_files[0]

    print(f"[1/5] Berkas Audio Vokal: {os.path.basename(audio_path)}")

    # 2. Muat model lokal offline atau model cache Whisper
    local_model_path = os.environ.get("WHISPER_MODEL_DIR", os.path.join(os.path.dirname(__file__), "..", "models", "whisper_small"))
    if not os.path.exists(local_model_path):
        local_model_path = "small"

    print(f"[2/5] Memuat model neural ({local_model_path})...")
    try:
        model = WhisperModel(local_model_path, device="cuda", compute_type="float16")
        print("      Akselerasi: NVIDIA GPU CUDA (Float16) Aktif!")
    except Exception as e:
        print(f"      Pemberitahuan CUDA ({e}), beralih ke: CPU INT8 Multi-thread.")
        model = WhisperModel(local_model_path, device="cpu", compute_type="int8")

    # 3. Transkripsi dengan word-level timestamps
    print(f"[3/5] Melakukan transkripsi neural & ekstraksi stempel waktu kata per kata...")
    segments, info = model.transcribe(audio_path, language="id", word_timestamps=True)

    all_segments = []
    all_words = []  # Kata per kata dengan stempel waktu riil
    full_transcript_text = []

    for seg in segments:
        full_transcript_text.append(seg.text.strip())
        seg_data = {
            "start": round(seg.start, 3),
            "end": round(seg.end, 3),
            "text": seg.text.strip(),
            "words": []
        }
        for w in seg.words:
            clean_w = normalize(w.word)
            if clean_w:
                w_obj = {
                    "word": w.word.strip(),
                    "clean": clean_w,
                    "start": round(w.start, 3),
                    "end": round(w.end, 3),
                    "prob": round(w.probability, 3)
                }
                seg_data["words"].append(w_obj)
                all_words.append(w_obj)
        all_segments.append(seg_data)

    total_audio_duration = all_words[-1]["end"] if all_words else 0.0
    print(f"      Total Kata Terdeteksi: {len(all_words)} kata")
    print(f"      Total Durasi Vokal: {total_audio_duration:.2f} detik")

    # Simpan naskah tutur lisan asli (Ground-Truth)
    naskah_out_path = os.path.join(prompts_dir, "naskah_elevenlabs_arunika.txt")
    with open(naskah_out_path, "w", encoding="utf-8") as f:
        f.write("\n".join(full_transcript_text))
    print(f"      Disimpan: naskah_elevenlabs_arunika.txt")

    # [BARU v2.0] 4. Ekspor word_timing_master.json — stempel waktu tiap kata riil
    # File ini digunakan oleh generate_kinetic_subtitles.js untuk Word-Level Snapping
    word_timing_path = os.path.join(prompts_dir, "word_timing_master.json")
    with open(word_timing_path, "w", encoding="utf-8") as f:
        json.dump(all_words, f, indent=2, ensure_ascii=False)
    print(f"[4/5] Disimpan: word_timing_master.json ({len(all_words)} kata dengan stempel waktu riil)")

    # 5. Buat atau perbarui sentence_timing_master.json (batas titik tengah kontinu)
    master_json_path = os.path.join(prompts_dir, "sentence_timing_master.json")

    # Baca naskah resmi jika ada di 04_prompts/naskah_full.txt (Anti Salah Dengar Whisper)
    official_script_path = os.path.join(prompts_dir, "naskah_full.txt")
    official_sentences = []
    if os.path.exists(official_script_path):
        try:
            with open(official_script_path, "r", encoding="utf-8-sig") as f:
                official_sentences = [line.strip().lstrip('\ufeff') for line in f if line.strip()]
        except Exception:
            pass

    print(f"[5/5] Menghitung & menyinkronkan sentence_timing_master.json dari segmen vokal aktif...")
    existing_timings = []
    n_segs = len(all_segments)

    for idx in range(n_segs):
        curr_seg = all_segments[idx]
        start_t = 0.0 if idx == 0 else existing_timings[idx - 1]["end"]

        if idx == n_segs - 1:
            end_t = round(total_audio_duration, 2)
        else:
            next_seg = all_segments[idx + 1]
            midpoint = (curr_seg["end"] + next_seg["start"]) / 2.0
            end_t = round(midpoint, 2)

        dur_t = round(end_t - start_t, 2)

        # Prioritaskan naskah resmi jika tersedia
        scene_text = curr_seg["text"]
        if official_sentences and len(official_sentences) == n_segs:
            scene_text = official_sentences[idx]
        elif official_sentences and idx < len(official_sentences):
            scene_text = official_sentences[idx]

        existing_timings.append({
            "scene": idx + 1,
            "clip": f"{idx + 1:02d}_scene.mp4",
            "topic": f"Adegan {idx + 1}",
            "start": round(start_t, 2),
            "end": round(end_t, 2),
            "speechStart": round(curr_seg["start"], 2),
            "speechEnd": round(curr_seg["end"], 2),
            "duration": dur_t,
            "setpts": round(dur_t / 7.98, 4),
            "text": scene_text
        })

    with open(master_json_path, "w", encoding="utf-8") as f:
        json.dump(existing_timings, f, indent=4, ensure_ascii=False)
    print(f"      Disimpan: sentence_timing_master.json ({len(existing_timings)} adegan, Durasi Total: {total_audio_duration:.2f}s)")

    # Ekspor SRT tanpa offset — waktu langsung dari audio, zero drift
    # [PERBAIKAN v2.0] Menggunakan stempel tutur kata riil agar tidak mendahului saat jeda diam
    srt_out_path = os.path.join(prompts_dir, "subtitles_id.srt")
    srt_lines = []

    for idx, item in enumerate(existing_timings, start=1):
        s_time = item.get("speechStart", item["start"])
        e_time = min(item.get("speechEnd", item["end"]) + 0.35, item["end"])
        s_srt = format_timestamp_srt(s_time)
        e_srt = format_timestamp_srt(e_time)

        text_content = item.get("text", item.get("topic", ""))
        srt_lines.append(f"{idx}\n{s_srt} --> {e_srt}\n{text_content}\n")


    with open(srt_out_path, "w", encoding="utf-8") as f:
        f.write("\n".join(srt_lines))
    print(f"      Disimpan: subtitles_id.srt (Zero-Offset, sinkron langsung dengan audio)")

    print(f"\n[SUKSES] Ekstraksi ground-truth timing selesai — Word-Level Snapping Siap!\n")
    print(f"  Berkas dihasilkan:")
    print(f"  - word_timing_master.json  ({len(all_words)} kata riil)")
    print(f"  - sentence_timing_master.json ({len(existing_timings)} adegan kalimat)")
    print(f"  - subtitles_id.srt (zero-offset sinkron audio)\n")

if __name__ == "__main__":
    target_dir = sys.argv[1] if len(sys.argv) > 1 else os.path.join(os.path.dirname(__file__), "..", "projects", "_test_sandbox_Uji_Mandiri")
    run_alignment(target_dir)
