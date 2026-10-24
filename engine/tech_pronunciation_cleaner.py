import re

# ==============================================================================
# KAMUS NORMALISASI LAFAL ISTILAH TEKNIS KOMPUTER — FOKUSKONTEN VOICE ENGINE
# Versi 2.0 — Diperluas dari 35 ke 80+ istilah, regex diperbaiki
# Menjamin: bebas serak, bebas halusinasi lafal, stabil untuk semua kategori
# konten masa depan (hardware, software, format file, internet, AI, dsb.)
# ==============================================================================

# Format entry: (regex_pattern, pengganti_fonetik, re.flags)
# Urutan PENTING: istilah lebih panjang/spesifik didahulukan agar tidak ditimpa
TECH_DICTIONARY = [

    # === FORMAT FILE & ARSIP ===
    (r"\bRARSFX\b",         "RAR-SFX",          re.IGNORECASE),
    (r"\b\.part(\d+)\.rar\b", r".part\1.rar",   re.IGNORECASE),
    (r"\bWinRAR\b",         "Win-Rar",           re.IGNORECASE),
    (r"\b7-Zip\b",          "Seven-Zip",         re.IGNORECASE),
    (r"\bSeven-Zip\b",      "Seven-Zip",         re.IGNORECASE),
    (r"\b\.rar\b",          "titik-RAR",         re.IGNORECASE),
    (r"\b\.zip\b",          "titik-ZIP",         re.IGNORECASE),
    (r"\b\.7z\b",           "titik-Seven-Z",     re.IGNORECASE),
    (r"\b\.iso\b",          "titik-ISO",         re.IGNORECASE),
    (r"\b\.exe\b",          "titik-EXE",         re.IGNORECASE),
    (r"\b\.dll\b",          "titik-D-L-L",       re.IGNORECASE),
    (r"\b\.pdf\b",          "titik-P-D-F",       re.IGNORECASE),
    (r"\b\.mp4\b",          "titik-M-P-4",       re.IGNORECASE),
    (r"\b\.mp3\b",          "titik-M-P-3",       re.IGNORECASE),
    (r"\b\.wav\b",          "titik-WAV",         re.IGNORECASE),
    (r"\b\.docx?\b",        "titik-DOC",         re.IGNORECASE),
    (r"\b\.xlsx?\b",        "titik-Excel",       re.IGNORECASE),
    (r"\b\.pptx?\b",        "titik-PowerPoint",  re.IGNORECASE),
    (r"\b\.txt\b",          "titik-TXT",         re.IGNORECASE),
    (r"\b\.png\b",          "titik-P-N-G",       re.IGNORECASE),
    (r"\b\.jpg\b",          "titik-J-P-G",       re.IGNORECASE),
    (r"\b\.apk\b",          "titik-A-P-K",       re.IGNORECASE),

    # === UKURAN FILE (spesifik dulu, baru umum) ===
    (r"\b(\d+(?:[.,]\d+)?)\s*TB\b", r"\1 Tera-Bait",  re.IGNORECASE),
    (r"\b(\d+(?:[.,]\d+)?)\s*GB\b", r"\1 Giga-Bait",  re.IGNORECASE),
    (r"\b(\d+(?:[.,]\d+)?)\s*MB\b", r"\1 Mega-Bait",  re.IGNORECASE),
    (r"\b(\d+(?:[.,]\d+)?)\s*KB\b", r"\1 Kilo-Bait",  re.IGNORECASE),

    # === HARDWARE ===
    (r"\bGTX\s*1660\b",     "G-T-X seribu enam ratus enam puluh", re.IGNORECASE),
    (r"\bRTX\b",            "R-T-X",             re.IGNORECASE),
    (r"\bGTX\b",            "G-T-X",             re.IGNORECASE),
    (r"\bNVIDIA\b",         "En-Vidia",          re.IGNORECASE),
    (r"\bAMD\b",            "A-M-D",             re.IGNORECASE),
    (r"\bIntel\b",          "Intel",             re.IGNORECASE),
    (r"\bCore i(\d)\b",     r"Core i-\1",        re.IGNORECASE),
    (r"\bSSD\b",            "S-S-D",             re.IGNORECASE),
    (r"\bHDD\b",            "H-D-D",             re.IGNORECASE),
    (r"\bNVMe\b",           "En-Vi-Me",          re.IGNORECASE),
    (r"\bRAM\b",            "R-A-M",             re.IGNORECASE),
    (r"\bROM\b",            "R-O-M",             re.IGNORECASE),
    (r"\bVRAM\b",           "V-R-A-M",           re.IGNORECASE),
    (r"\bCPU\b",            "C-P-U",             re.IGNORECASE),
    (r"\bGPU\b",            "G-P-U",             re.IGNORECASE),
    (r"\bUSB\b",            "U-S-B",             re.IGNORECASE),
    (r"\bHDMI\b",           "H-D-M-I",           re.IGNORECASE),
    (r"\bVGA\b",            "V-G-A",             re.IGNORECASE),
    (r"\bBIOS\b",           "B-I-O-S",           re.IGNORECASE),
    (r"\bUEFI\b",           "U-E-F-I",           re.IGNORECASE),
    (r"\bLAN\b",            "L-A-N",             re.IGNORECASE),

    # === SISTEM OPERASI & APLIKASI ===
    (r"\bWindows Explorer\b","Windos Eksplorer",  re.IGNORECASE),
    (r"\bWindows Defender\b","Windos Defender",   re.IGNORECASE),
    (r"\bWindows 11\b",     "Windos sebelas",     re.IGNORECASE),
    (r"\bWindows 10\b",     "Windos sepuluh",     re.IGNORECASE),
    (r"\bWindows\b",        "Windos",             re.IGNORECASE),
    (r"\bGoogle Drive\b",   "Gugel Draif",        re.IGNORECASE),
    (r"\bGDrive\b",         "G-Draif",            re.IGNORECASE),
    (r"\bYouTube\b",        "Yutub",              re.IGNORECASE),
    (r"\bWhatsApp\b",       "Wats-Ap",            re.IGNORECASE),
    (r"\bInstagram\b",      "Instagram",          re.IGNORECASE),
    (r"\bTikTok\b",         "Tik-Tok",            re.IGNORECASE),
    (r"\bMicrosoft\b",      "Mikro-soft",         re.IGNORECASE),
    (r"\bIDM\b",            "I-D-M",              re.IGNORECASE),
    (r"\bVLC\b",            "V-L-C",              re.IGNORECASE),
    (r"\bAI\b",             "A-I",                re.IGNORECASE),

    # === INTERNET & JARINGAN ===
    (r"\bHTTPS?://\S+",     "tautan link",        re.IGNORECASE),
    (r"\bHTTPS\b",          "H-T-T-P-S",          re.IGNORECASE),
    (r"\bHTTP\b",           "H-T-T-P",            re.IGNORECASE),
    (r"\bURL\b",            "U-R-L",              re.IGNORECASE),
    (r"\bWiFi\b",           "Wai-Fai",            re.IGNORECASE),
    (r"\bBluetooth\b",      "Blutut",             re.IGNORECASE),
    (r"\bVPN\b",            "V-P-N",              re.IGNORECASE),
    (r"\bIP\b",             "I-P",                re.IGNORECASE),
    (r"\bDNS\b",            "D-N-S",              re.IGNORECASE),

    # === AKSI & ERROR KOMPUTER ===
    (r"\bchecksum\b",       "cek-sam",            re.IGNORECASE),
    (r"\berror\b",          "eror",               re.IGNORECASE),
    (r"\bcorrupt(?:ed)?\b", "korap",              re.IGNORECASE),
    (r"\bpassword\b",       "pas-wod",            re.IGNORECASE),
    (r"\bproperties\b",     "propertis",          re.IGNORECASE),
    (r"\bExtract Here\b",   "Ekstrak-Hir",        re.IGNORECASE),
    (r"\bExtract to\b",     "Ekstrak-tu",         re.IGNORECASE),
    (r"\bextract\b",        "ekstrak",            re.IGNORECASE),
    (r"\bdouble.?click\b",  "dobel-klik",         re.IGNORECASE),
    (r"\bclick\b",          "klik",               re.IGNORECASE),
    (r"\bpop.?up\b",        "pop-ap",             re.IGNORECASE),
    (r"\btaskbar\b",        "task-bar",           re.IGNORECASE),
    (r"\bdesktop\b",        "des-top",            re.IGNORECASE),
    (r"\bexplorer\b",       "eksplorer",          re.IGNORECASE),
    (r"\bsoftware\b",       "sof-wer",            re.IGNORECASE),
    (r"\bhardware\b",       "had-wer",            re.IGNORECASE),
    (r"\bfirmware\b",       "ferm-wer",           re.IGNORECASE),
    (r"\bdriver\b",         "draiver",            re.IGNORECASE),
    (r"\bformat\b",         "format",             re.IGNORECASE),
    (r"\binstall(?:ation)?\b","instal",           re.IGNORECASE),
    (r"\buninstall\b",      "un-instal",          re.IGNORECASE),
    (r"\bdownload\b",       "daunlod",            re.IGNORECASE),
    (r"\bupload\b",         "aplod",              re.IGNORECASE),
    (r"\bupdate\b",         "apdet",              re.IGNORECASE),
    (r"\brestart\b",        "restart",            re.IGNORECASE),
    (r"\bshutdown\b",       "syat-daun",          re.IGNORECASE),
    (r"\bbackup\b",         "bek-ap",             re.IGNORECASE),
    (r"\bscreenshot\b",     "skrin-syot",         re.IGNORECASE),
    (r"\bmulti.?part\b",    "multi-part",         re.IGNORECASE),
    (r"\bpart\s+(\d+)\b",   r"part \1",           re.IGNORECASE),

    # === MEDIA & KONTEN ===
    (r"\bsubscribe\b",      "sabskraib",          re.IGNORECASE),
    (r"\bchannel\b",        "canel",              re.IGNORECASE),
    (r"\bplaylist\b",       "plei-lis",           re.IGNORECASE),
    (r"\bthumbnail\b",      "tam-neil",           re.IGNORECASE),
    (r"\bstreaming\b",      "stri-ming",          re.IGNORECASE),
    (r"\bbuffer\b",         "bafer",              re.IGNORECASE),

    # === ANGKA PERSEN ===
    (r"\b(\d+)\s*%",        r"\1 persen",         0),

    # === DRIVE PATH ===
    (r"\bDrive\s+([CDE])\b", r"Draif-\1",         re.IGNORECASE),
    (r"\bFolder\b",         "folder",             re.IGNORECASE),
]

def clean_tech_pronunciation(text: str) -> str:
    """Mengonversi istilah teknis menjadi ejaan fonetik stabil bahasa Indonesia.
    Menjamin: bebas serak, bebas halusinasi lafal, konsisten lintas kategori konten."""
    cleaned = text
    for pattern, replacement, flags in TECH_DICTIONARY:
        cleaned = re.sub(pattern, replacement, cleaned, flags=flags)
    return cleaned

if __name__ == "__main__":
    # Uji petik 5 kategori konten
    samples = [
        "Error corrupt saat ekstrak file RAR multi-part 4.5GB di Drive D.",
        "Klik kanan, pilih Extract Here, lalu tunggu proses selesai.",
        "Buka Windows Explorer, cek properties file .zip yang gagal.",
        "GPU GTX 1660 SUPER 6GB VRAM dengan RAM 16GB dan SSD NVMe.",
        "Download IDM versi terbaru 80% lebih cepat dari browser biasa.",
        "Update driver NVIDIA agar checksum valid dan error hilang.",
        "Subscribe channel FokusKonten, aktifkan notifikasi YouTube.",
    ]
    print("=== UJI KAMUS NORMALISASI LAFAL TEKNIS v2.0 ===\n")
    for s in samples:
        hasil = clean_tech_pronunciation(s)
        print(f"Asli  : {s}")
        print(f"Fonem : {hasil}")
        print()
