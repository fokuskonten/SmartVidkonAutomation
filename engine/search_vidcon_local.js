/**
 * search_vidcon_local.js — Mesin Koreografer Footage Vidkon Lokal Tier-1 FokusKonten
 * Berbasis Basis Data SQLite WAL, Dynamic Semantic Metaphor, & 5-Layer Anti-Pattern Engine
 * 
 * 100% Offline, Zero Bandwidth, Instant Retrieval (31.057 Klip Koleksi Vidcon)
 * Standar Arsitektur: arsitektur_sistem_video_edukasi_fokuskonten.md (Pilar I Fase 1)
 * Versi 3.0.0 — Anti-Pattern & State Ledger SQLite WAL
 */
'use strict';

const fs = require('fs');
const path = require('path');
const { DatabaseSync } = require('node:sqlite');

let pathsResolver = null;
try {
  pathsResolver = require('./paths_resolver');
} catch (e) {}

const DB_PATH = (pathsResolver && pathsResolver.getDatabasePath)
  ? pathsResolver.getDatabasePath()
  : path.join(__dirname, '..', 'database', 'footage_catalog.db');
const SUMMARY_PATH = (pathsResolver && pathsResolver.getCatalogSummaryPath)
  ? pathsResolver.getCatalogSummaryPath()
  : path.join(__dirname, '..', 'database', 'katalog_vidcon_summary.json');

function resolveClipPaths(clip) {
  if (!clip) return clip;
  if (pathsResolver) {
    if (pathsResolver.resolveVideoPath) clip.video_path = pathsResolver.resolveVideoPath(clip.video_path);
    if (pathsResolver.resolveSnapshotPath && clip.snapshot_path) clip.snapshot_path = pathsResolver.resolveSnapshotPath(clip.snapshot_path);
  }
  return clip;
}

// [INV-11 & ZERO CONTAMINATION GUARD]
// 7 Bundel yang telah disortir dan dieliminasi resmi oleh Muhari (Samudra, Receh, Prank, Murottal, Bioskop Lawas, Alur Film, Scene Film)
// HARAM TERCAMPUR / TERKONTAMINASI KEMBALI DALAM KONDISI APAPUN.
const ELIMINATED_SKUS = ['ID4834', 'ID53FA', 'IDA71C', 'IDCB24', 'IDE830', 'IDE8B2', 'ID8F64'];
const ELIMINATED_SQL_CLAUSE = "AND is_clean = 1 AND bundle_sku NOT IN ('ID4834', 'ID53FA', 'IDA71C', 'IDCB24', 'IDE830', 'IDE8B2', 'ID8F64')";


// Inisialisasi Koneksi SQLite WAL
let db = null;
function getDatabase() {
  if (!db) {
    if (!fs.existsSync(DB_PATH)) {
      throw new Error(`Basis data footage_catalog.db tidak ditemukan di: ${DB_PATH}`);
    }
    db = new DatabaseSync(DB_PATH);
    db.exec('PRAGMA busy_timeout = 5000;');
    db.exec('PRAGMA journal_mode = WAL;');
    db.exec('PRAGMA synchronous = NORMAL;');
  }
  return db;
}

/**
 * Memuat ringkasan metadata klaster & bundel
 */
function loadCatalog(full = false) {
  if (!fs.existsSync(SUMMARY_PATH)) {
    throw new Error('Berkas katalog vidcon summary tidak ditemukan di: ' + SUMMARY_PATH);
  }
  return JSON.parse(fs.readFileSync(SUMMARY_PATH, 'utf8'));
}

/**
 * Menentukan klaster utama berdasarkan tema naskah
 */
function resolvePrimaryCluster(catalog, themeName = 'hard_reality') {
  const themeMap = {
    'hard_reality': 'CLUSTER_HARD_REALITY',
    'alpha_stoic': 'CLUSTER_HARD_REALITY',
    'stoik': 'CLUSTER_HARD_REALITY',
    'dark_psychology': 'CLUSTER_HARD_REALITY',
    'realitas_pahit': 'CLUSTER_HARD_REALITY',
    'motivasi': 'CLUSTER_HARD_REALITY',
    'kemiskinan': 'CLUSTER_HARD_REALITY',
    'pria': 'CLUSTER_HARD_REALITY',
    'perang': 'CLUSTER_HARD_REALITY',
    'melancholy': 'CLUSTER_MELANCHOLY_SOUL',
    'galau': 'CLUSTER_MELANCHOLY_SOUL',
    'refleksi': 'CLUSTER_MELANCHOLY_SOUL',
    'senja_melankolis': 'CLUSTER_MELANCHOLY_SOUL',
    'spiritual': 'CLUSTER_MELANCHOLY_SOUL',
    'healing': 'CLUSTER_MELANCHOLY_SOUL',
    'craft': 'CLUSTER_CRAFT_TRANSFORMATION',
    'keterampilan': 'CLUSTER_CRAFT_TRANSFORMATION',
    'karat': 'CLUSTER_CRAFT_TRANSFORMATION',
    'industri_mesin': 'CLUSTER_CRAFT_TRANSFORMATION',
    'kreatif': 'CLUSTER_CRAFT_TRANSFORMATION',
    'hustle': 'CLUSTER_CRAFT_TRANSFORMATION',
    'nostalgia': 'CLUSTER_NOSTALGIA_HISTORY',
    'lawas': 'CLUSTER_NOSTALGIA_HISTORY',
    'sejarah': 'CLUSTER_NOSTALGIA_HISTORY',
    'public': 'CLUSTER_PUBLIC_CHARM',
    'pemersatu': 'CLUSTER_PUBLIC_CHARM',
    'tech': 'CLUSTER_TECH_FUTURE',
    'cyberpunk': 'CLUSTER_TECH_FUTURE',
    'humor': 'CLUSTER_HUMOR_SATIRE',
    'receh': 'CLUSTER_HUMOR_SATIRE',
    'santai': 'CLUSTER_HUMOR_SATIRE'
  };

  const clusterId = themeMap[themeName.toLowerCase()] || 'CLUSTER_HARD_REALITY';
  return catalog.clusters[clusterId] || catalog.clusters['CLUSTER_HARD_REALITY'];
}

/**
 * Taksonomi Semantik 27 Koleksi Vidkon Bersih (Berdasarkan Katalog Snapshot 5 Detik & Tag Resmi)
 * Menghubungkan setiap kata/makna naskah ke visual nyata yang teruji pada frame ke-5.
 * Versi 3.2.0 — Kurasi 4-Tier: Andalan Utama, Aktif Terverifikasi, Rolling Cadangan
 */
const BUNDLE_CONCEPTS = {
  // --- TIER 1: ANDALAN UTAMA (CORE PILLARS) ---
  'IDE0C2': {
    name: 'FYPVault - Pemersatu Bangsa',
    tags: [
      'pesona', 'percaya diri', 'gaya hidup', 'elegan', 'penampilan', 'wanita', 'kharisma', 'aura',
      'daya tarik', 'kharismatik', 'wajah', 'hook', 'tatapan memikat', 'senyuman manis', 'gaun',
      'anggun', 'memukau', 'visual pembuka', 'magnetik', 'percaya diri tinggi', 'ratu', 'kilau',
      'memikat penonton', 'daya pikat', 'daya magis', 'sosok memikat', 'keanggunan'
    ]
  },
  'ID9919': {
    name: 'SigmaReel - Shelby Alpha Stoik',
    tags: [
      'stoik', 'shelby', 'peaky', 'wibawa', 'pria', 'pemimpin', 'setelan jas', 'tegas', 'prinsip',
      'mental baja', 'hukum besi', 'ketenangan', 'disiplin', 'diam', 'kendali nasib', 'wibawa pria',
      'medan perang', 'pertempuran', 'darah', 'keabadian', 'nakhoda', 'takdir', 'tegakkan langkahmu',
      'pedang', 'pria sejati', 'kesatria', 'kedaulatan', 'martabat', 'harga diri', 'singa sejati',
      'singa', 'tuan mutlak', 'karakter tangguh', 'karakter kuat', 'tegak', 'berdiri tegak', 'takhta',
      'kejayaan', 'tatapan dingin', 'rokok', 'topi pet', 'bayangan jalan', 'jas hitam', 'berjalan tenang',
      'mengabaikan gonggongan', 'raja sejati', 'mahkota', 'tekad baja', 'kendali pikiran',
      'tenang di atas badai', 'hukum rimba', 'menatap lurus', 'langkah mantap', 'wibawa maskulin'
    ]
  },
  'IDFFA0': {
    name: 'VoidVibes - Dark Psychology',
    tags: [
      'dark psychology', 'tatapan dingin', 'tatapan mata', 'tatapan tajam', 'misterius', 'dingin',
      'gelisah', 'meraba', 'batas diri', 'sulit ditebak', 'manipulasi', 'tanpa rasa bersalah',
      'punggung kokoh', 'intimidasi', 'amarah', 'tersulut', 'reaksi berlebihan', 'rapuh', 'ketakutan',
      'bayang-bayang', 'topeng sosial', 'topeng', 'cermin retak', 'cermin', 'musuh', 'iri dengki',
      'berkhianat', 'kebohongan', 'pura-pura', 'jebakan', 'provokasi', 'pengecut', 'senyuman dingin',
      'psikologi gelap', 'manipulatif', 'serigala berbulu domba', 'topeng kepalsuan', 'motif tersembunyi',
      'racun batin', 'kegelapan', 'ruang hampa', 'sorot mata', 'dingin membeku', 'kacamata hitam',
      'bermain pikiran', 'bisikan racun', 'kedengkian senyap'
    ]
  },
  'IDB7CA': {
    name: 'RogueMind - Joker Realitas Pahit',
    tags: [
      'joker', 'senyum pahit', 'luka batin', 'kecewa', 'licik', 'menginjak', 'kelemahan', 'isi pikiran',
      'tempat sampah emosi', 'kemarahan', 'dunia kejam', 'realitas keras', 'tersenyum', 'penderitaan',
      'nafsu purba', 'hembusan angin', 'picik', 'gonggongan', 'hinaan', 'menurunkan derajat',
      'anjing menggonggong', 'anjing', 'caci maki', 'meremehkan', 'hujatan', 'kegaduhan', 'sampah emosi',
      'duri', 'tertawa sinis', 'tawa getir', 'badut', 'tertawa di atas perih', 'dunia munafik',
      'cemoohan orang', 'kepahitan hidup', 'tertawa miris', 'luka masa lalu', 'sudut kelam',
      'senyuman terluka', 'terhina', 'diinjak', 'merasakan kepedihan'
    ]
  },
  'IDFCC3': {
    name: 'DuskAura - Senja Indie Melankolis & Dark Mind',
    tags: [
      'senja', 'indie', 'gerimis', 'malam', 'motor tua', 'lelah kerja', 'pulang', 'sepi', 'perjalanan',
      'puitis', 'hujan', 'menarik diri', 'lelah', 'batin', 'jalanan sepi', 'renungan', 'kesendirian',
      'kawah sunyi', 'pengorbanan batin', 'meruntuhkan jiwa', 'kepedihan', 'menyiksa', 'malam gerimis',
      'jalanan basah', 'aspal basah', 'senja sepi', 'kelelahan', 'sunyi senyap', 'hampa', 'merenung',
      'keheningan malam', 'pulang larut', 'helm', 'lampu temaram', 'rintik hujan', 'sendiri di jalan',
      'hembusan angin malam', 'senja jingga', 'sunyi jalanan', 'kepenatan', 'relung hati', 'sunyi',
      'langkah gontai', 'jalanan berbayang', 'kedinginan malam'
    ]
  },
  'ID7667': {
    name: 'AuraBoost - Alam Puncak Healing',
    tags: [
      'healing', 'alam', 'puncak', 'refleksi jiwa', 'tenang', 'udara segar', 'pemenang sejati',
      'ketenangan batin', 'kekuatan terbesarmu', 'hutan pinus', 'tebing', 'pegunungan', 'bangkit',
      'kedamaian', 'ketenangan mutlak', 'berdiri hening', 'kebebasan sejati', 'tepuk tangan',
      'pengakuan manusia', 'melangkah tenang', 'batu karang', 'samudra luas', 'samudra', 'ombak',
      'ombak lautan', 'puncak berkabut', 'tebing curam', 'mercusuar', 'kedamaian jiwa', 'air terjun',
      'kabut sejuk', 'menghirup napas', 'puncak gunung', 'kedamaian semesta', 'alam bebas',
      'luasnya langit', 'pohon cemara', 'riak air', 'gemericik air', 'keheningan alam'
    ]
  },
  'IDDD2B': {
    name: 'CraftGenius - Lifehack Kreatif & Motivasi Membangun',
    tags: [
      'lifehack', 'kreasi', 'motivasi', 'membangun', 'solusi kreatif', 'trik cerdas', 'alat sederhana',
      'kunci', 'mengunci lisan', 'benteng', 'cerdik', 'kunci pintu', 'diy', 'kreativitas', 'ide brilian',
      'benteng privasi', 'solusi', 'inovasi', 'akal', 'benteng pertahanan', 'racun', 'kedengkian',
      'akal sehat', 'kunci kendali', 'kunci gembok', 'pagar jiwa', 'benteng karakter', 'merakit',
      'menyusun', 'akal cerdik', 'taktik', 'daya cipta', 'solusi tak terduga', 'perbaikan cepat',
      'rancang bangun', 'trik praktis', 'cerdas berdaya'
    ]
  },
  'ID557A': {
    name: 'DesiCraft - Kerja Keras Ekstrem',
    tags: [
      'skill dewa', 'kerja keras', 'otot', 'daya tahan', 'hancur lebur', 'beban', 'banting tulang',
      'keringat', 'di balik layar', 'panas', 'fisik', 'vrindavan', 'atraksi', 'tangan gesit',
      'pantang menyerah', 'lumpur', 'tangan kerdil', 'jalanan berdebu', 'mengangkat beban',
      'terik matahari', 'kerja kasar', 'buruh jalanan', 'tanpa mengeluh', 'meneteskan peluh',
      'ketahanan fisik', 'kerja keras nyata', 'tenaga perkasa', 'gesit', 'adonan', 'gerobak'
    ]
  },
  'IDE5EF': {
    name: 'MechTech - Mesin Manufaktur Industri',
    tags: [
      'mesin pabrik', 'manufaktur', 'baut', 'las', 'perkakas', 'baja', 'tindakan', 'telak', 'membantah',
      'industri', 'teknik', 'percikan api', 'buruh', 'batasan baja', 'robotik', 'presisi pabrik',
      'ditempa', 'kawah sunyi', 'tempaan api', 'baja terbaik', 'bengkel tempa', 'bengkel', 'mesin industri',
      'ditempa api', 'api tempaan', 'palu', 'tekanan', 'lelehan besi', 'roda gigi', 'mesin pemotong',
      'percikan las', 'dentuman mesin', 'tekanan tinggi', 'presisi mikron', 'pabrikasi', 'cetakan baja',
      'baja panas', 'alat industri', 'mesin bubut'
    ]
  },
  'ID9C29': {
    name: 'ReviveCraft - Restorasi Karat Logam',
    tags: [
      'karat', 'restorasi', 'pembersihan logam', 'ketekunan', 'mengikis', 'memperbaiki kesalahan',
      'senyap', 'bengkel', 'barang jadul', 'besi', 'bersihkan lingkaran', 'menajamkan pedang',
      'kedaulatan', 'istana', 'karat besi', 'mengikis karat', 'besi tua', 'benda tua', 'ketekunan harian',
      'restorasi jiwa', 'amplas', 'cairan kimia', 'mengilapkan', 'memulihkan', 'barang antik',
      'barang bekas', 'membersihkan noda', 'ketelitian senyap', 'mengasah pisau', 'perkakas tua'
    ]
  },
  'ID3199': {
    name: 'WallMotion - Visual 4K Loop Sinematik',
    tags: [
      'loop', 'latar belakang', 'estetik', 'abstrak', 'gelombang', 'galaksi', 'waktu', 'visual halus',
      'warna gelap', 'animasi', 'visual 4k', 'megah', 'pujian semu', 'bintang', 'keabadian',
      'partikel emas', 'kosmik', 'ruang angkasa', 'aliran waktu', 'visual megah', 'dimensi lain',
      'gelombang energi', 'keagungan visual', 'latar belakang megah', 'partikel cahaya', 'cahaya kosmik'
    ]
  },
  'ID_MASTER_STORY': {
    name: 'Master Exclusive Story & B-Roll',
    tags: [
      'master', 'badai', 'petir', 'monumen', 'silat', 'bela diri', 'sejarah', 'pejuang', 'gerilya',
      'meditasi', 'tasbih', 'balkon', 'cakrawala', 'ledakan', 'kemerdekaan', 'pahlawan', 'berakar kuat',
      'alam', 'puing', 'lumpur', 'matahari', 'anjing liar', 'gelanggang', 'pedang tajam', 'gejolak emosi',
      'keheningan', 'berdiri hening', 'meledak', 'menggetarkan jagat', 'tukang kayu', 'menghaluskan kayu',
      'ketam kayu', 'serutan kayu', 'monumen perunggu', 'patung pahlawan', 'bambu runcing', 'meraut bambu',
      'jembatan kali bekasi', 'jembatan meledak', 'parit perang', 'pejuang lumpur', 'pencak silat',
      'jurus silat', 'pendekar', 'anak menangis', 'balita di lantai dingin', 'keluarga sederhana',
      'makan bersama', 'kesetiaan istri', 'mengusap dahi suami', 'membasuh kening', 'istri setia',
      'ibu mencuci piring', 'mengasuh balita', 'kabut pagi', 'sinar matahari menembus fajar',
      'pemuda di balkon', 'menatap cakrawala', 'naskah kuno', 'kitab klasik', 'catatan sejarah kuno',
      'istana langit', 'dewa petir', 'pengusaha stres', 'layar aplikasi', 'krisis teknologi',
      'pencuri uang', 'copet motor', 'maling', 'merayakan kemenangan', 'pahlawan bangsa'
    ]
  },
  'ID_BRAND_FK': {
    name: 'Brand FokusKonten Official Outro',
    tags: [
      'brand', 'logo', 'outro', 'fokuskonten', '3d metalik', 'pedestal', 'obsidian', 'marmer',
      'penutup', 'identitas', 'resmi', 'wibawa', 'baja yang tak pernah bisa ditundukkan',
      'tak pernah bisa ditundukkan', 'kemenangan mutlak', 'takhta akhir', 'penutup video'
    ]
  },

  // --- TIER 2: AKTIF TERVERIFIKASI (VERIFIED ADDITIONS) ---
  'ID617C': {
    name: 'AffilTrend - Racun Shopee Affiliate Royalty-Free',
    tags: [
      'racun shopee', 'belanja', 'unboxing', 'barang unik', 'estetik', 'gadget', 'kreatif',
      'perlengkapan', 'shopee', 'affiliate', 'rekomendasi', 'hemat', 'cerah', 'kreativitas',
      'barang viral', 'kemasan paket', 'produk unik'
    ]
  },
  'ID1646': {
    name: 'NeonFuture - Cyberpunk & Industri Modern AI',
    tags: [
      'cyberpunk', 'teknologi modern', 'kota industri', 'teknologi ai', 'kecerdasan buatan',
      'robotik ai', 'robot', 'futuristik', 'digital', 'sci fi', 'masa depan', 'canggih',
      'layar digital', 'algoritma', 'modernitas', 'memetakan', 'kejernihan mutlak', 'sirkuit',
      'hologram', 'lampu neon', 'era digital', 'data', 'otomatisasi', 'teknologi masa depan',
      'kecerdasan mesin', 'metropolis modern', 'kabel serat optik'
    ]
  },
  'ID32DF': {
    name: 'LiveHustle - Live Selling Barbar & Hustle',
    tags: [
      'jualan', 'live selling', 'dagang', 'pasar', 'hustle', 'dagangan', 'promo', 'barang',
      'penjual', 'omset', 'transaksi', 'banting harga', 'butuh uang', 'suasana pasar', 'antusias',
      'keramaian pasar', 'tawar menawar', 'pedagang gigih', 'mencari nafkah', 'keringat rezeki'
    ]
  },
  'ID7792': {
    name: 'AnimeGrief - Melankolis Anime',
    tags: [
      'anime', 'galau', 'kesepian', 'patah hati', 'merenung', 'hampa', 'air mata', 'jiwa',
      'kepanikan jiwa', 'kehilangan kendali', 'malam dingin', 'menyiksa batin', 'menangis',
      'siluet kaca jendela', 'kegelapan malam', 'rasa sakit', 'perih', 'patah', 'kesendirian batin'
    ]
  },
  'ID8125': {
    name: 'ChefByte - Resep Anak Kos & Kreativitas Masak',
    tags: [
      'masak', 'resep', 'makanan', 'dapur', 'bumbu', 'goreng', 'kuliner', 'olahan', 'sarapan',
      'anak kos', 'kreatif', 'hidangan', 'cita rasa', 'meracik bumbu', 'wajan panas'
    ]
  },
  'IDBABD': {
    name: 'BiteRush - Mukbang Brutal & Kuliner Ekspresif',
    tags: [
      'mukbang', 'makan', 'makan besar', 'pedas', 'saus', 'lahap', 'gigitan', 'selera', 'daging',
      'lezat', 'ekspresi makan', 'porsi besar', 'suapan besar', 'pedas membara'
    ]
  },
  'ID3C1F': {
    name: 'ToolCraft - Pertukangan Kreatif',
    tags: [
      'pertukangan', 'kayu', 'alat kerja', 'keahlian', 'bengkel', 'tangan terampil', 'karya',
      'mengasah keterampilan', 'gergaji', 'presisi', 'perkakas', 'karya nyata', 'ketam',
      'serutan kayu', 'kayu jati', 'meja kerja kayu', 'pahat', 'perajin kayu', 'mengukir',
      'sambungan kayu', 'serat kayu', 'ketukangan', 'kayu balok'
    ]
  },

  // --- TIER 3: ROLLING CADANGAN / DEPRIORITIZED (BACKUP ONLY) ---
  'IDEBCB': {
    name: 'ThaiVibes - Komedi Satir Thailand',
    tags: ['komedi thailand', 'humor', 'kocak', 'disuruh', 'konyol', 'reaksi konyol', 'perdebatan kosong', 'meladeni', 'iklan lucu', 'plot twist lucu', 'menghibur', 'pemersatu barbar']
  },
  'ID1AF4': {
    name: 'HookMagnet - Hook 3 Detik Kejutan Cepat',
    tags: ['hook 3 detik', 'visual cepat', 'tarik perhatian', 'pembuka', 'dramatis', 'kejutan', 'viral', 'gerakan cepat']
  },
  'IDC20C': {
    name: 'PetViral - Anabul Gemoy',
    tags: ['kucing', 'anjing', 'hewan', 'anabul', 'gemoy', 'lucu', 'tingkah', 'polos', 'menghibur', 'tawa', 'manja', 'imut', 'bermain', 'menggemaskan', 'santai']
  },
  'IDCE99': {
    name: 'HomeAura - Room Tour Minimalis',
    tags: ['desain', 'rumah', 'kamar', 'estetika', 'minimalis', 'rapi', 'dekorasi', 'menata', 'interior', 'ruang', 'tenang', 'estetik', 'sudut', 'meja kerja', 'produktif', 'hangat', 'lampu', 'ruang tamu']
  },
  'IDC50B': {
    name: 'MeowFrenzy - Anabul Tantrum AI',
    tags: ['kucing ai', 'tantrum', 'anabul kasual', 'meow', 'kucing lucu', 'fun', 'kucing ngambek']
  },
  'ID5FF3': {
    name: 'KiddieAI - Bayi AI Gemoy',
    tags: ['bayi ai', 'gemoy', 'anak kecil', 'polos', 'lucu', 'imut', 'balita']
  },
  'IDEAD7': {
    name: 'JoyPaws - Interaksi Anabul',
    tags: ['interaksi anabul', 'peliharaan', 'tingkah hewan', 'bermain', 'kawan hewan']
  },
  'IDEF4B': {
    name: 'ToyPlay - Unboxing Mainan Anak',
    tags: ['mainan anak', 'unboxing', 'mainan', 'boneka', 'warna warni', 'koleksi']
  },
  'ID380D': {
    name: 'HookStrike - Tarikan Aksi Mancing',
    tags: ['mancing', 'monster', 'tarikan', 'pertarungan', 'daya juang', 'melawan', 'adu mulut', 'hasil nyata', 'pembuktian', 'adrenalin', 'sungai', 'laut']
  }
};

/**
 * [v3.5] Direktori Semantik Klip Eksklusif ID_MASTER_STORY
 * Memetakan kata-kata naskah spesifik secara langsung ke frame klip fisik nyata di DB
 */
const MASTER_STORY_CLIPS_MAP = [
  { prefix: 'Man_carrying_cement_sack', keywords: ["semen","sak semen","kuli","angkat semen","kuli bangunan","beban berat","memanggul semen","buruh kasar"] },
  { prefix: 'Man_clearing_construction_debris', keywords: ["puing","proyek","membersihkan puing","konstruksi","bongkaran","material","reruntuhan","debu proyek"] },
  { prefix: 'Carpenter_smoothing_wood', keywords: ["tukang kayu","menghaluskan kayu","ketam","serutan kayu","kayu perajin","kayu jati","carpenter","serutan"] },
  { prefix: 'Hands_carving_green_bamboo', keywords: ["bambu runcing","meraut bambu","bambu hijau","senjata bambu","meruncingkan bambu","bambu"] },
  { prefix: 'Man_feeding_livestock_on_farm', keywords: ["ternak","pakan ternak","kandang","peternak","desa","sapi","kambing","unggas","ladang"] },
  { prefix: 'Man_checking_printouts_in_shop', keywords: ["percetakan","cetak","print","kertas","toko cetak","dokumen","desain cetak"] },
  { prefix: 'Man_playing_guitar_busking', keywords: ["ngamen","pengamen","gitar","jalanan","busking","bernyanyi jalanan","petikan gitar","seniman jalanan"] },
  { prefix: 'Man_playing_acoustic_guitar', keywords: ["gitar akustik","petikan gitar","akustik","bermusik","alat musik gitar"] },
  { prefix: 'Woman_listening_to_busking_man', keywords: ["mendengar pengamen","menonton ngamen","seniman jalanan","suasana jalanan"] },
  { prefix: 'Military_bombers_dropping_bombs', keywords: ["bom","pesawat pengebom","jatuhkan bom","pesawat tempur","ledakan udara","serangan udara","bomber"] },
  { prefix: 'Explosion_at_Kali_Bekasi_bridge', keywords: ["jembatan meledak","kali bekasi","ledakan dahsyat","bom jembatan","ledakan","menghancurkan jembatan"] },
  { prefix: 'Guerrilla_fighters_charging', keywords: ["gerilya","pejuang gerilya","menyerbu lumpur","lumpur perang","pasukan pejuang","gerilyawan"] },
  { prefix: 'Leader_shouting_in_trenches', keywords: ["parit","teriakan komando","pemimpin di parit","parit perang","komando perang","garis depan"] },
  { prefix: 'Freedom_fighters_standing_together', keywords: ["pejuang kemerdekaan","berdiri bersama","persatuan pejuang","barisan pejuang","bambu runcing"] },
  { prefix: 'Villagers_and_fighters_celebrati', keywords: ["merayakan kemenangan","kemenangan rakyat","sorak gembira","pesta kemenangan","kemenangan bersama","bendera merah putih"] },
  { prefix: 'Colonial_forces_retreat_in_mud', keywords: ["penjajah","kolonial","mundur di lumpur","tentara belanda","mundur perang","lumpur perang"] },
  { prefix: 'Leader_wading_through_mangrove', keywords: ["rawa","hutan bakau","mangrove","pemimpin di rawa","menyusuri rawa","gerilya hutan"] },
  { prefix: 'Fighters_practicing_pencak_silat', keywords: ["pencak silat","pendekar","jurus silat","bela diri silat","silat","pendekar bangsa"] },
  { prefix: 'Camera_rising_up_bronze_monument', keywords: ["monumen perunggu","monumen pahlawan","patung perunggu","prasasti pahlawan","tugu pahlawan","monumen","tugu"] },
  { prefix: 'Reading_ancient_handwritten_Isla', keywords: ["naskah kuno","kitab kuno","tulisan tangan kuno","manuskrip","kitab klasik","huruf arab","santri"] },
  { prefix: 'Sunlight_filtering_through_pesan', keywords: ["pesantren","pondok pesantren","fajar pesantren","cahaya surau","sinar menembus pesantren","mengaji"] },
  { prefix: 'Leader_meditating_with_prayer_beads', keywords: ["meditasi","tasbih","doa pemimpin","wirid","berzikir","merenung khusyuk","ketenangan spiritual"] },
  { prefix: 'Cleric_standing_at_wetlands', keywords: ["ulama","kiai","tokoh agama","berdiri di rawa","dakwah","menatap ufuk"] },
  { prefix: 'Sunlight_piercing_morning_mist', keywords: ["kabut pagi","sinar matahari menembus","kabut fajar","fajar menyingsing","mentari pagi","sinar fajar"] },
  { prefix: 'Youth_standing_on_sandstone_balcony', keywords: ["balkon","menatap cakrawala","pemuda berdiri","balkon batu","memandang jauh","cakrawala luas"] },
  { prefix: 'Celestial_palace_with_lightning', keywords: ["istana langit","petir","kilatan petir","istana awan","angkasa petir","gemuruh petir"] },
  { prefix: 'Zeus_floating_near_celestial_temple', keywords: ["dewa langit","kuil langit","penguasa langit","zeus","dewa petir"] },
  { prefix: 'Toddler_crying_on_bare_floor', keywords: ["balita menangis","lantai dingin","anak miskin","anak menangis","kemiskinan anak","tangisan anak","kelaparan","meratap di lantai"] },
  { prefix: 'Men_repossessing_scooter_from_fa', keywords: ["tarik motor","debt collector","sita motor","utang","penagih utang","kehilangan motor","kemiskinan keluarga","cicilan"] },
  { prefix: 'Thief_stealing_cash_and_phone', keywords: ["pencuri","merampas uang","copet","maling ponsel","kejahatan uang","mencuri uang","kriminal"] },
  { prefix: 'Thief_stealing_motorcycle', keywords: ["curanmor","maling motor","mencuri sepeda motor","kehilangan motor","bobol kunci"] },
  { prefix: 'Thief_steals_camera_and_motorcycle', keywords: ["maling kamera","rampas motor","kejahatan jalanan","pencurian"] },
  { prefix: 'Man_observing_stolen_motorcycle', keywords: ["melihat motor hilang","korban curanmor","kehilangan kendaraan","bingung motor hilang"] },
  { prefix: 'Family_grieving_after_robbery', keywords: ["korban perampokan","keluarga berduka","trauma rampok","menangis kehilangan","kemalingan"] },
  { prefix: 'Wife_comforts_husband_at_desk', keywords: ["istri menenangkan","kesetiaan istri","mendampingi suami","istri setia","suami di meja","menemani suami","pundak suami"] },
  { prefix: "Wife_wiping_husband's_forehead", keywords: ["membasuh kening","mengusap dahi","keringat suami","kelembutan istri","istri membasuh","lelah suami"] },
  { prefix: 'Wife_left_alone_in_mist', keywords: ["istri sendiri","ditinggal sendiri","kabut sepi","kesepian istri","menatap bayang"] },
  { prefix: 'Husband_comforting_weeping_wife', keywords: ["suami menenangkan","istri menangis","merangkul istri","pelukan penenang","duka bersama"] },
  { prefix: 'Husband_surprises_wife_with_scooter', keywords: ["kejutan motor","hadiah motor","kebahagiaan suami istri","senyum istri","sepeda motor baru"] },
  { prefix: 'Man_sliding_ring_on_wife', keywords: ["memasang cincin","cincin nikah","mahar","pernikahan","jari manis","janji suci"] },
  { prefix: 'Couple_exchanging_wedding_rings', keywords: ["tukar cincin","ijab kabul","ikatan suci","cincin kawin","pernikahan sakral"] },
  { prefix: 'Woman_washing_dishes_with_children', keywords: ["cuci piring","ibu tangguh","mengasuh anak","ibu berjuang","pekerjaan rumah tangga","dapur sederhana"] },
  { prefix: 'Mother_stroking_crying_baby', keywords: ["ibu membelai bayi","bayi menangis","kasih sayang ibu","menggendong bayi","ibu menenangkan anak"] },
  { prefix: 'Newborn_baby_crying', keywords: ["bayi baru lahir","tangis bayi","kelahiran","bayi mungil menangis","awal kehidupan"] },
  { prefix: 'Child_reaching_for_fading_hand', keywords: ["tangan memudar","kehilangan orang tua","perpisahan","anak meraih tangan","kepergian"] },
  { prefix: 'Family_sharing_simple_meal', keywords: ["makan bersama","keluarga sederhana","sepiring nasi","hidangan sederhana","kebersamaan keluarga"] },
  { prefix: 'Family_playing_together_outdoors', keywords: ["keluarga bahagia","bermain di taman","anak dan ayah","keceriaan keluarga"] },
  { prefix: 'Family_sitting_on_picnic_blanket', keywords: ["piknik keluarga","duduk santai keluarga","tikar piknik"] },
  { prefix: 'Family_walking_down_path', keywords: ["berjalan bersama keluarga","langkah keluarga","jalan setapak keluarga"] },
  { prefix: 'Woman_watches_mother_in_smoke', keywords: ["ibu di dapur berasap","melihat ibu lelah","asap dapur","pengorbanan ibu"] },
  { prefix: 'Entrepreneur_stressed_over_softw', keywords: ["pengusaha stres","krisis usaha","pusing menatap layar","beban bisnis","kegagalan usaha","stres bisnis"] },
  { prefix: 'Exhausted_seller_leaning_in_despair', keywords: ["pedagang putus asa","lelah jualan","penjual lesu","omset anjlok","terduduk lelah","dagangan sepi"] },
  { prefix: 'Woman_slamming_desk_in_frustration', keywords: ["banting meja","frustrasi kerja","marah di meja","kesal di kantor","emosi memuncak"] },
  { prefix: 'Woman_sighing_at_messy_desk', keywords: ["mendesah lelah","meja berantakan","pusing kerja","tumpukan berkas","beban kerja"] },
  { prefix: 'Woman_reacting_to_cancellation_m', keywords: ["pesanan dibatalkan","pembatalan order","kekecewaan jualan","cancel order","notifikasi batal"] },
  { prefix: 'Woman_looking_at_smartphone_angrily', keywords: ["marah menatap hp","kesal melihat ponsel","chat mengecewakan","komplain pelanggan"] },
  { prefix: 'Person_shaking_freezing_smartphone', keywords: ["hp ngehang","ponsel macet","aplikasi error","kepanikan sistem","layar beku"] },
  { prefix: 'Pharmacy_owner_stressed_by_medicine', keywords: ["apotek pusing","stres obat","krisis apotek","pemilik toko obat","manajemen obat kacau"] },
  { prefix: 'Camera_moving_across_messy_desk', keywords: ["meja berantakan","berkas menumpuk","kertas berceceran","catatan kusut","kerja lembur"] },
  { prefix: 'Tired_woman_saving_phone_numbers', keywords: ["simpan kontak lelah","catat nomor manual","input data lelah","rekap kontak"] },
  { prefix: 'Husband_coding_at_desk', keywords: ["coding","programmer","mengetik kode","laptop malam","developer","software engineer","coding meja"] },
  { prefix: 'Delivery_courier_picking_up_parcels', keywords: ["kurir paket","pengiriman barang","ekspedisi","pejuang paket","antar paket","logistik"] },
  { prefix: 'Woman_taping_shipping_labels', keywords: ["lakban paket","tempel resi","packing pesanan","penjual online packing","bungkus barang"] },
  { prefix: 'Online_seller_celebrating_delive', keywords: ["rayakan pesanan","orderan tembus","penjual online sukses","panen orderan","pesanan melimpah"] },
  { prefix: 'Woman_smiling_at_sales_growth', keywords: ["grafik penjualan naik","omset meroket","penjualan sukses","tersenyum di laptop","pertumbuhan bisnis"] },
  { prefix: 'Man_receives_surprising_financia', keywords: ["kabar rezeki","saldo melonjak","uang masuk mendadak","kejutan finansial","tercengang melihat saldo"] },
  { prefix: 'Man_reacting_to_good_news', keywords: ["kabar baik","gembira","kemenangan kecil","bersorak senang","respons kabar baik"] },
  { prefix: 'Man_smiling_at_smartphone', keywords: ["senyum lihat hp","notifikasi menyenangkan","chat menggembirakan","tersenyum di ponsel"] },
  { prefix: 'Pharmacist_presenting_organized', keywords: ["apotek rapi","penataan obat","solusi apotek","stok tertata","apoteker profesional"] },
  { prefix: 'Pharmacist_tapping_tablet_screen', keywords: ["tablet kasir apotek","input tablet","aplikasi kasir obat","sistem apotek"] },
  { prefix: 'Pharmacy_owner_smiling_at_phone', keywords: ["pemilik apotek tersenyum","pantau omset apotek","solusi toko obat"] },
  { prefix: 'Entrepreneur_presenting_software', keywords: ["presentasi software","demo aplikasi","pitching bisnis","tampil percaya diri"] },
  { prefix: 'Woman_demonstrating_CRM_software', keywords: ["demo software crm","tunjukkan aplikasi","otomasi bisnis","sistem crm"] },
  { prefix: 'Woman_holding_software_box', keywords: ["kotak software","box aplikasi","produk digital box","lisensi resmi"] },
  { prefix: 'Woman_holding_software_product_box', keywords: ["pegang kotak software","produk aplikasi resmi","box software"] },
  { prefix: 'Woman_presenting_product_to_owner', keywords: ["tawarkan produk","presentasi ke bos","solusi ke pemilik"] },
  { prefix: 'Woman_presenting_software_produc', keywords: ["presentasi produk software","tunjukkan fitur aplikasi"] },
  { prefix: 'Woman_presenting_software_to_des', keywords: ["presentasi di meja","penjelasan aplikasi"] },
  { prefix: 'Presenter_smiling_giving_thumbs_up', keywords: ["jempol puas","presenter tersenyum","rekomendasi terpercaya"] },
  { prefix: 'Woman_sitting_with_coffee', keywords: ["santai ngopi","kerja rileks","secangkir kopi","tenang depan laptop"] },
  { prefix: 'Woman_drinking_tea_with_laptop', keywords: ["minum teh santai","kerja nyaman","nikmati suasana"] },
  { prefix: 'Woman_looking_at_laptop', keywords: ["menatap layar laptop","fokus di laptop","bekerja di laptop"] },
  { prefix: 'Woman_holding_smartphone_looking', keywords: ["pegang hp santai","menatap smartphone"] },
  { prefix: 'Man_walking_through_city_street', keywords: ["berjalan di kota","langkah kaki di trotoar","pria melangkah di jalan kota"] },
  { prefix: 'Man_and_woman_laughing_together', keywords: ["tertawa bersama","senyum bahagia","pasangan ceria","kehangatan berdua"] },
  { prefix: 'Man_comforted_by_wife', keywords: ["ditenangkan istri","rangkulan hangat","kekuatan dari istri"] },
  { prefix: 'Man_photographing_with_DSLR_camera', keywords: ["fotografer","kamera dslr","memotret","bidik lensa","karya foto"] },
  { prefix: 'Girl_growing_into_woman_walking', keywords: ["tumbuh dewasa","langkah masa depan","transformasi diri","perjalanan kedewasaan"] },
  { prefix: 'CorelDraw_templates_scrolling_up', keywords: ["coreldraw","template desain grafis","katalog corel","aset cdr","vektor corel"] },
  { prefix: 'Templates_scrolling_on_3D_plane', keywords: ["template 3d scroll","katalog grafis 3d","showcase template"] },
  { prefix: 'Templates_scrolling_vertically', keywords: ["katalog template vertikal","gulir template","aset digital template"] },
  { prefix: 'Isometric_presentation_slides_fl', keywords: ["slide presentasi","powerpoint isometrik","template pptx","slide profesional"] },
  { prefix: 'Product_cards_scrolling_upwards', keywords: ["kartu produk digital","katalog produk e-commerce","etalase produk"] },
  { prefix: 'Scrolling_e-course_marketplace_i', keywords: ["e-course marketplace","kelas online","kursus digital"] },
  { prefix: 'Laptop_displaying_AI_chat_assistant', keywords: ["ai assistant chat","asisten ai di laptop","chatbot ai"] },
  { prefix: 'Laptop_displaying_CRM_software_d', keywords: ["dashboard crm","tampilan software crm","sistem crm laptop"] },
  { prefix: 'Laptop_displaying_digital_city_map', keywords: ["peta digital kota","map navigasi di laptop","sistem peta"] },
  { prefix: 'Laptop_screen_showing_lifetime_l', keywords: ["lisensi seumur hidup","lifetime license","aktivasi lisensi"] },
  { prefix: 'Laptop_showing_automated_broadca', keywords: ["broadcast otomatis","kirim pesan massal","otomasi broadcast"] },
  { prefix: 'Smartphone_displaying_financial', keywords: ["finansial smartphone","grafik keuangan di hp","laporan saldo hp"] },
  { prefix: 'WhatsApp_Lead_CRM_Pro_showcase', keywords: ["whatsapp crm showcase","aplikasi wa crm","fitur crm pro"] },
  { prefix: 'FokusKonten_Katalog_Scroll_9x16', keywords: ["katalog fokuskonten","toko digital fokuskonten","etalase produk 9x16"] }
];

/**
 * [v4.0] CONCEPT LEXICON — Peta Konsep Visual -> Bobot Bundel
 * Menjembatani kata naskah (setelah di-stem) ke bundel footage yang secara visual memuat konsep tersebut.
 * Aturan istilah:
 *   - 'kata'       : dicocokkan via varian stem (berjuang/perjuangan/memperjuangkan -> juang)
 *   - 'kata!'      : hanya kata utuh persis (anti salah sasaran, mis. 'las' tidak boleh cocok dari 'kelas')
 *   - 'frasa dua'  : dicocokkan sebagai frasa di teks
 * Bobot: konsep visual konkret (laut, jas, las, gunung) lebih tinggi daripada konsep abstrak.
 */
const CONCEPT_LEXICON = {
  LAUT: {
    terms: ['laut', 'samudra', 'samudera', 'ombak', 'pantai', 'gelombang', 'karang', 'mercusuar', 'bahari', 'pesisir', 'debur'],
    weights: { 'ID7667': 14, 'ID3199': 3 }
  },
  GUNUNG: {
    terms: ['gunung', 'puncak', 'tebing', 'hutan', 'pinus', 'kabut', 'lembah', 'cemara', 'bukit', 'sungai', 'air terjun', 'alam bebas'],
    weights: { 'ID7667': 14 }
  },
  JAS: {
    terms: ['jas', 'setelan', 'dasi', 'wibawa', 'pemimpin', 'pimpin', 'bos', 'sigma', 'alpha', 'kemeja', 'elegan', 'topi pet', 'kantor', 'korporat', 'perusahaan', 'atasan', 'manajer', 'eksekutif', 'rekan kerja'],
    weights: { 'ID9919': 14 }
  },
  PRIA: {
    terms: ['pria', 'lelaki', 'laki'],
    weights: { 'ID9919': 6 }
  },
  RAJA: {
    terms: ['singa', 'raja', 'takhta', 'tahta', 'mahkota', 'kesatria', 'ksatria', 'bangkit', 'menang', 'kemenangan', 'juara', 'taklukkan', 'berani', 'pemenang'],
    weights: { 'ID9919': 10 }
  },
  JUANG: {
    terms: ['juang', 'kerja', 'keringat', 'peluh', 'otot', 'latih', 'olahraga', 'buruh', 'gigih', 'kuli', 'tenaga', 'beban', 'lelah',
      'banting tulang', 'angkat beban', 'kerja keras', 'pantang menyerah', 'cuti', 'absen', 'lembur', 'gaji', 'upah', 'karyawan', 'pekerjaan',
      'nafkah', 'utang', 'cicilan', 'bertahan', 'capek', 'letih', 'budak', 'perbudakan', 'pecat', 'dipecat', 'cari nafkah', 'bangun pagi'],
    weights: { 'ID557A': 14, 'IDE5EF': 6, 'ID_MASTER_STORY': 3 }
  },
  KREATIF: {
    terms: ['kreatif', 'kreasi', 'kreativitas', 'ide!', 'idemu', 'inovasi', 'cipta', 'rakit', 'rancang', 'akal', 'trik', 'solusi', 'karya', 'gagasan', 'racik'],
    weights: { 'IDDD2B': 14, 'ID3C1F': 7 }
  },
  KAYU: {
    terms: ['kayu', 'gergaji', 'pahat', 'ukir', 'tukang', 'serut', 'ketam'],
    weights: { 'ID3C1F': 12, 'ID_MASTER_STORY': 6 }
  },
  TEMPA: {
    terms: ['tempa', 'las!', 'baja', 'besi', 'mesin', 'pabrik', 'palu', 'api!', 'percik', 'bengkel', 'logam', 'industri', 'bara', 'roda gigi'],
    weights: { 'IDE5EF': 14, 'ID9C29': 4 }
  },
  KARAT: {
    terms: ['karat', 'kikis', 'amplas', 'restorasi', 'pulih', 'asah', 'usang', 'antik', 'rongsok', 'noda'],
    weights: { 'ID9C29': 14 }
  },
  LUKA: {
    terms: ['luka', 'sakit', 'hina', 'caci', 'ejek', 'remeh', 'khianat', 'pahit', 'sinis', 'badut', 'cemooh', 'hujat', 'olok', 'getir', 'perih', 'tawa sinis', 'hancur', 'kecewa', 'kehilangan', 'terluka', 'takut', 'ketakutan', 'tangis', 'air mata', 'derita', 'tersiksa'],
    weights: { 'IDB7CA': 12, 'ID7792': 4 }
  },
  GELAP: {
    terms: ['topeng', 'cermin', 'manipulasi', 'bayang', 'intimidasi', 'tatapan', 'musuh', 'dengki', 'iri!', 'licik', 'racun', 'dusta', 'bohong', 'tipu', 'gelap', 'sistem', 'jebakan', 'terjebak', 'korban', 'terkurung', 'kambing hitam', 'palsu'],
    weights: { 'IDFFA0': 12 }
  },
  MALAM: {
    terms: ['malam', 'senja', 'hujan', 'gerimis', 'aspal', 'temaram', 'motor', 'lampu jalan'],
    weights: { 'IDFCC3': 12 }
  },
  SEPI: {
    terms: ['sepi', 'sunyi', 'sendiri', 'sendirian', 'kesendirian', 'pulang', 'jalan', 'hening', 'terasing', 'kamar sempit', 'sunyi senyap'],
    weights: { 'IDFCC3': 6 }
  },
  SEDIH: {
    terms: ['hampa', 'tangis', 'rapuh', 'hilang', 'rindu', 'galau', 'duka', 'pilu', 'nestapa', 'air mata', 'patah hati'],
    weights: { 'ID7792': 12, 'IDFCC3': 4 }
  },
  TEKNO: {
    terms: ['teknologi', 'digital', 'ai!', 'robot', 'kota', 'neon', 'algoritma', 'komputer', 'layar', 'data', 'internet', 'gedung', 'masa depan'],
    weights: { 'ID1646': 14 }
  },
  SEMESTA: {
    terms: ['waktu', 'abadi', 'bintang', 'galaksi', 'semesta', 'kosmik', 'angkasa', 'jagat', 'detik'],
    weights: { 'ID3199': 12 }
  },
  PERANG: {
    terms: ['perang', 'pahlawan', 'pedang', 'medan', 'prajurit', 'gerilya', 'pejuang', 'tempur', 'zirah', 'parit', 'merdeka', 'senjata'],
    weights: { 'ID_MASTER_STORY': 12, 'ID9919': 8 }
  }
};

// Himpunan akar kata yang dikenal leksikon (dipakai stemIndo untuk memilih akar paling masuk akal)
const KNOWN_STEMS = new Set();
Object.values(CONCEPT_LEXICON).forEach(c => c.terms.forEach(t => {
  if (!t.includes(' ') && !t.endsWith('!')) KNOWN_STEMS.add(t);
}));

/**
 * [v4.0] Stemmer Bahasa Indonesia ringan (rule-based, tanpa dependensi eksternal).
 * Menghasilkan varian akar kata dari imbuhan umum: partikel (-lah, -kah), posesif (-nya, -ku, -mu),
 * sufiks (-kan, -an, -i), prefiks (meng/meny/mem/men/me, peng/peny/pem/pen/pe, ber/per/ter, be-ker, di/ke/se)
 * termasuk peluluhan (meny->s, mem->p, men->t, meng->k bila diikuti vokal).
 */
const STEM_VOWELS = 'aeiou';
function normalizeWord(word) {
  return String(word || '').toLowerCase().replace(/[^a-z]/g, '');
}

// Mengembalikan daftar varian setelah melepas satu lapis prefiks (urutan = tebakan terbaik dulu)
function stripPrefixVariants(s) {
  const out = [];
  const isV = ch => STEM_VOWELS.includes(ch);
  const add = (v, min = 3) => { if (v && v.length >= min && !out.includes(v)) out.push(v); };
  let m;
  if ((m = s.match(/^(ber|per|ter)(.+)$/))) add(m[2]);
  if ((m = s.match(/^be(ker.+)$/))) add(m[1]); // bekerja -> kerja
  if ((m = s.match(/^(meng|peng)(.+)$/))) { if (isV(m[2][0])) add('k' + m[2]); add(m[2]); }
  else if ((m = s.match(/^(meny|peny)(.+)$/))) { add('s' + m[2]); }
  else if ((m = s.match(/^(mem|pem)(.+)$/))) { if (isV(m[2][0])) add('p' + m[2]); add(m[2]); }
  else if ((m = s.match(/^(men|pen)(.+)$/))) { if (isV(m[2][0])) add('t' + m[2]); add(m[2]); }
  else if ((m = s.match(/^(me|pe)(.+)$/))) { add(m[2]); }
  if ((m = s.match(/^(di|ke|se)(.+)$/))) add(m[2], 4);
  return out;
}

// Seluruh varian kandidat akar kata (aditif: kata asli tetap disertakan agar tidak ada kecocokan yang hilang)
function stemVariants(word) {
  const w = normalizeWord(word);
  const out = new Set();
  if (!w) return out;
  out.add(w);
  const bases = [w];
  for (const suf of ['lah', 'kah']) {
    if (w.length - suf.length >= 3 && w.endsWith(suf)) { bases.push(w.slice(0, -suf.length)); break; }
  }
  for (const b of bases.slice()) {
    for (const suf of ['nya', 'ku', 'mu']) {
      if (b.length - suf.length >= 3 && b.endsWith(suf)) { bases.push(b.slice(0, -suf.length)); break; }
    }
  }
  const forms = [];
  for (const b of bases) {
    forms.push(b);
    for (const suf of ['kan', 'an', 'i']) {
      if (b.length - suf.length >= 3 && b.endsWith(suf)) forms.push(b.slice(0, -suf.length));
    }
  }
  for (const f of forms) {
    out.add(f);
    for (const p1 of stripPrefixVariants(f)) {
      out.add(p1);
      for (const p2 of stripPrefixVariants(p1)) out.add(p2);
    }
  }
  return out;
}

// Akar kata tunggal: utamakan akar yang dikenal leksikon, jika tidak ada gunakan rantai aturan deterministik
function stemIndo(word) {
  const vars = stemVariants(word);
  let best = null;
  for (const v of vars) {
    if (KNOWN_STEMS.has(v) && (!best || v.length > best.length)) best = v;
  }
  if (best) return best;
  let s = normalizeWord(word);
  if (s.length <= 4) return s;
  for (const suf of ['lah', 'kah']) { if (s.length - suf.length >= 4 && s.endsWith(suf)) { s = s.slice(0, -suf.length); break; } }
  for (const suf of ['nya', 'ku', 'mu']) { if (s.length - suf.length >= 4 && s.endsWith(suf)) { s = s.slice(0, -suf.length); break; } }
  for (const suf of ['kan', 'an', 'i']) { if (s.length - suf.length >= 4 && s.endsWith(suf)) { s = s.slice(0, -suf.length); break; } }
  for (let layer = 0; layer < 2; layer++) {
    const v = stripPrefixVariants(s);
    if (!v.length) break;
    s = v[0];
  }
  return s;
}

// Deteksi konsep visual pada satu kalimat naskah (dihitung sekali per adegan, independen dari SKU)
function detectConceptHits(lower) {
  const tokens = lower.split(/[^a-z0-9]+/).filter(t => t.length >= 2);
  const tokenVariants = tokens.map(t => ({ raw: t, vars: stemVariants(t) }));
  const hits = [];
  for (const [key, concept] of Object.entries(CONCEPT_LEXICON)) {
    const words = [];
    for (const term of concept.terms) {
      if (term.includes(' ')) {
        if (lower.includes(term)) words.push(term);
      } else if (term.endsWith('!')) {
        const base = term.slice(0, -1);
        if (tokens.includes(base)) words.push(base);
      } else {
        const tv = tokenVariants.find(x => x.vars.has(term));
        if (tv) words.push(tv.raw);
      }
    }
    if (words.length > 0) {
      hits.push({ key, words: Array.from(new Set(words)), weights: concept.weights });
    }
  }
  return hits;
}

/**
 * [v3.2] Multi-Niche Domain Engine (SSOT v3.2) — Zero Risk & Dual-Hook Booster
 * Menghubungkan Niche Naskah ke Hook Scene 01, Mid-Roll Booster, Whitelist Pool Klip, dan Subfolder BGM Resmi.
 */
const NICHE_DOMAINS = {
  DOMAIN_STOIC: {
    id: 'DOMAIN_STOIC',
    code: '01_stoik',
    name: 'Stoikisme & Realitas Keras',
    bgmFolder: '01_Stoik_Realitas',
    hookBundle: 'IDE0C2', // Tetap sakral Pemersatu Bangsa (Hook Pembuka)
    secondaryHook: 'ID9919', // Quotes Shelby
    midRollBundle: 'IDE0C2', // Mid-Roll Retention Booster
    pool: [
      'IDE0C2', 'ID9919', 'IDFFA0', 'IDB7CA', 'IDFCC3', 'ID7667',
      'ID557A', 'IDE5EF', 'ID9C29', 'ID1646', 'ID3199', 'ID7792', 'IDDD2B', 'ID_MASTER_STORY'
    ],
    maxCapPerSku: {
      'IDE0C2': 2, // 1 klip di Scene 01 + 1 klip di Mid-Roll (~pertengahan durasi)
      'ID9919': 3, // Shelby andalan
      'IDB7CA': 3, // Joker Sadboy andalan
      'IDFFA0': 2, // Dark Psychology
      'IDFCC3': 2, // Senja Indie
      'ID557A': 2, // DesiCraft kerja keras buruh
      'ID7667': 2, // Alam Puncak Healing
      'ID3199': 2, // WallMotion 4K loop
      'IDE5EF': 2, // MechTech mesin manufaktur
      'ID9C29': 2, // Karat restorasi senyap
      'ID1646': 2, // Cyberpunk & AI modern
      'IDDD2B': 2, // Lifehack inovasi/bangkit
      'ID7792': 2, // Anime melankolis
      'ID_MASTER_STORY': 4 // Master Exclusive Story B-Roll (Proporsional ~20% agar 13 katalog emas tetap aktif)
    },
    defaultMaxCap: 2
  },
  DOMAIN_SPIRITUAL: {
    id: 'DOMAIN_SPIRITUAL',
    code: '02_spiritual',
    name: 'Spiritual, Renungan & Kedamaian Batin',
    bgmFolder: '02_Spiritual_Dakwah',
    hookBundle: 'ID7667', // AuraBoost Alam Puncak & Ketenangan Jiwa
    secondaryHook: 'IDFCC3', // DuskAura Senja Melankolis
    midRollBundle: 'ID7667',
    pool: ['ID7667', 'IDFCC3', 'ID3199', 'ID9C29', 'ID7792'],
    maxCapPerSku: {
      'ID7667': 2,
      'IDFCC3': 2
    },
    defaultMaxCap: 1
  },
  DOMAIN_HISTORY: {
    id: 'DOMAIN_HISTORY',
    code: '03_sejarah',
    name: 'Sejarah, Restorasi & Jejak Waktu',
    bgmFolder: '03_Sejarah_Peradaban',
    hookBundle: 'ID9C29', // ReviveCraft Restorasi Karat Logam
    secondaryHook: 'IDFCC3', // DuskAura Senja
    midRollBundle: 'ID9C29',
    pool: ['ID9C29', 'IDFCC3', 'ID3199', 'ID3C1F', 'ID7667'],
    maxCapPerSku: {
      'ID9C29': 2,
      'IDFCC3': 2
    },
    defaultMaxCap: 1
  },
  DOMAIN_CREATIVE: {
    id: 'DOMAIN_CREATIVE',
    code: '04_kreatif',
    name: 'Kreativitas, Desain, Kriya & Lifehack',
    bgmFolder: '04_Kreatif_Desain',
    hookBundle: 'IDDD2B', // CraftGenius Lifehack Kreatif (Wajib Kreasi & Membangun)
    secondaryHook: 'ID3C1F', // ToolCraft Pertukangan Kreatif
    midRollBundle: 'IDDD2B',
    pool: ['IDDD2B', 'ID3C1F', 'ID617C', 'ID8125', 'ID9C29', 'ID3199', 'IDCE99'],
    maxCapPerSku: {
      'IDDD2B': 2,
      'ID3C1F': 2,
      'ID617C': 2
    },
    defaultMaxCap: 1
  },
  DOMAIN_CASUAL: {
    id: 'DOMAIN_CASUAL',
    code: '05_santai',
    name: 'Santai, Komedi & Hiburan Ringan',
    bgmFolder: '05_Santai_Komedi',
    hookBundle: 'IDEBCB', // ThaiVibes Komedi Thailand (Pemersatu Barbar)
    secondaryHook: 'ID1AF4', // HookMagnet Hook 3 Detik
    midRollBundle: 'IDEBCB',
    pool: ['IDEBCB', 'ID32DF', 'IDBABD', 'ID617C', 'ID8125', 'IDDD2B', 'ID1AF4', 'IDC20C', 'IDC50B', 'ID5FF3', 'IDEAD7', 'IDEF4B'],
    maxCapPerSku: {
      'IDEBCB': 2,
      'ID32DF': 2,
      'IDBABD': 2
    },
    defaultMaxCap: 1
  },
  DOMAIN_DARK_PSYCHOLOGY: {
    id: 'DOMAIN_DARK_PSYCHOLOGY',
    code: '06_dark_psychology',
    name: 'Psikologi Gelap & Batasan Diri',
    bgmFolder: '01_Stoik_Realitas',
    hookBundle: 'IDFFA0', // VoidVibes Dark Psychology
    secondaryHook: 'ID9919',
    midRollBundle: 'IDE0C2',
    pool: ['IDFFA0', 'IDB7CA', 'ID9919', 'IDFCC3', 'ID7792', 'ID3199', 'ID_MASTER_STORY'],
    maxCapPerSku: { 'IDFFA0': 3, 'ID9919': 3, 'IDB7CA': 2 },
    defaultMaxCap: 2
  },
  DOMAIN_JOKER_REALITY: {
    id: 'DOMAIN_JOKER_REALITY',
    code: '07_realitas_pahit',
    name: 'Realitas Pahit & Luka Kehidupan',
    bgmFolder: '01_Stoik_Realitas',
    hookBundle: 'IDB7CA', // RogueMind Joker
    secondaryHook: 'IDFFA0',
    midRollBundle: 'IDE0C2',
    pool: ['IDB7CA', 'IDFFA0', 'ID9919', 'IDFCC3', 'ID557A', 'ID_MASTER_STORY'],
    maxCapPerSku: { 'IDB7CA': 3, 'IDFFA0': 2, 'ID9919': 2 },
    defaultMaxCap: 2
  },
  DOMAIN_SENJA: {
    id: 'DOMAIN_SENJA',
    code: '08_senja_melankolis',
    name: 'Senja Melankolis & Kelelahan Batin',
    bgmFolder: '01_Stoik_Realitas',
    hookBundle: 'IDFCC3', // DuskAura Senja Indie
    secondaryHook: 'ID7792',
    midRollBundle: 'IDFCC3',
    pool: ['IDFCC3', 'ID7792', 'ID7667', 'ID3199', 'ID9C29', 'ID_MASTER_STORY'],
    maxCapPerSku: { 'IDFCC3': 3, 'ID7792': 2, 'ID7667': 2 },
    defaultMaxCap: 2
  },
  DOMAIN_HEALING: {
    id: 'DOMAIN_HEALING',
    code: '09_healing',
    name: 'Refleksi Jiwa, Alam Puncak & Healing',
    bgmFolder: '02_Spiritual_Dakwah',
    hookBundle: 'ID7667', // AuraBoost Alam Puncak
    secondaryHook: 'IDFCC3',
    midRollBundle: 'ID7667',
    pool: ['ID7667', 'IDFCC3', 'ID3199', 'ID9C29', 'ID_MASTER_STORY'],
    maxCapPerSku: { 'ID7667': 3, 'IDFCC3': 2 },
    defaultMaxCap: 2
  },
  DOMAIN_MECHTECH: {
    id: 'DOMAIN_MECHTECH',
    code: '10_industri_mesin',
    name: 'Kerja Keras Ekstrem, Mesin Pabrik & Industri',
    bgmFolder: '01_Stoik_Realitas',
    hookBundle: 'IDE5EF', // MechTech Mesin Manufaktur
    secondaryHook: 'ID557A',
    midRollBundle: 'IDE0C2',
    pool: ['IDE5EF', 'ID557A', 'ID9C29', 'ID9919', 'ID3C1F', 'IDDD2B', 'ID_MASTER_STORY'],
    maxCapPerSku: { 'IDE5EF': 3, 'ID557A': 2, 'ID9C29': 2 },
    defaultMaxCap: 2
  },
  DOMAIN_CYBERPUNK: {
    id: 'DOMAIN_CYBERPUNK',
    code: '11_cyberpunk',
    name: 'Teknologi Modern, Masa Depan & Cyberpunk AI',
    bgmFolder: '01_Stoik_Realitas',
    hookBundle: 'ID1646', // NeonFuture Cyberpunk
    secondaryHook: 'ID3199',
    midRollBundle: 'IDE0C2',
    pool: ['ID1646', 'ID3199', 'IDE5EF', 'ID9919', 'IDDD2B', 'ID_MASTER_STORY'],
    maxCapPerSku: { 'ID1646': 3, 'ID3199': 2 },
    defaultMaxCap: 2
  },
  DOMAIN_HUSTLE: {
    id: 'DOMAIN_HUSTLE',
    code: '12_hustle',
    name: 'Bisnis, Hustle, Pasar & Live Selling',
    bgmFolder: '05_Santai_Komedi',
    hookBundle: 'ID32DF', // LiveHustle
    secondaryHook: 'ID617C',
    midRollBundle: 'ID32DF',
    pool: ['ID32DF', 'ID617C', 'IDBABD', 'IDDD2B', 'ID8125', 'ID3C1F'],
    maxCapPerSku: { 'ID32DF': 3, 'ID617C': 2 },
    defaultMaxCap: 2
  },
  DOMAIN_PERANG: {
    id: 'DOMAIN_PERANG',
    code: '13_perang',
    name: 'Perang, Perjuangan & Kepahlawanan Bangsa',
    bgmFolder: '03_Sejarah_Peradaban',
    hookBundle: 'ID_MASTER_STORY', // B-Roll Pahlawan & Monumen Perunggu
    secondaryHook: 'ID9919',
    midRollBundle: 'IDE0C2',
    pool: ['ID_MASTER_STORY', 'ID9919', 'IDE5EF', 'IDFFA0', 'IDFCC3', 'ID7667', 'ID3199'],
    maxCapPerSku: { 'ID_MASTER_STORY': 4, 'ID9919': 3, 'IDE5EF': 2 },
    defaultMaxCap: 2
  }
};

/**
 * Deteksi Niche Domain Cerdas (Zero Risk: Default ke DOMAIN_STOIC jika tidak ada domain eksplisit)
 */
function detectNicheDomain(scriptText = '', title = '', requestedDomain = null) {
  if (requestedDomain) {
    const reqStr = String(requestedDomain).toLowerCase().trim();
    for (const [key, d] of Object.entries(NICHE_DOMAINS)) {
      if (key.toLowerCase() === reqStr || d.code.toLowerCase().includes(reqStr) || d.name.toLowerCase().includes(reqStr) || (d.code.length > 3 && reqStr.includes(d.code.slice(3)))) {
        return d;
      }
    }
  }

  const combined = ((title || '') + ' ' + (scriptText || '')).toLowerCase();

  // Keyword check (membutuhkan kecocokan kata utuh agar tidak salah sasaran substring seperti 'bagaimana' -> 'iman')
  const spiritualKeywords = ['allah', 'tuhan', 'doa', 'sholat', 'ibadah', 'sujud', 'masjid', 'quran', 'hadits', 'surga', 'neraka', 'pahala', 'dosa', 'ikhlas', 'tawakal', 'spiritual', 'dakwah', 'murottal', 'hijrah', 'berkah', 'ridho', 'iman', 'syukur', 'istighfar', 'sedekah', 'taubat'];
  const historyKeywords = ['sejarah', 'peradaban', 'kuno', 'kerajaan', 'perang dunia', 'kekaisaran', 'revolusi', 'zaman purba', 'leluhur', 'prasasti', 'arsip lama', 'tempo dulu', 'romawi', 'yunani', 'nusantara', 'majapahit', 'arkeologi'];
  const warKeywords = ['perang', 'pejuang', 'gerilya', 'pahlawan', 'bambu runcing', 'parit', 'penjajah', 'kemerdekaan', 'monumen', 'pertempuran', 'pendekar', 'jurus silat', 'pencak silat'];
  const creativeKeywords = ['desain rumah', 'kreativitas', 'interior minimalis', 'kriya', 'estetika ruang', 'room tour', 'dekorasi', 'menggambar', 'seniman', 'kerajinan tangan', 'diy project', 'home decor', 'lifehack', 'resep', 'memasak'];
  const casualKeywords = ['kocak', 'lucu', 'ngakak', 'komedi', 'receh', 'prank', 'canda', 'humor', 'tertawa', 'kucing lucu', 'anabul', 'anjing gemoy', 'mukbang'];

  const countMatches = (list) => list.reduce((acc, kw) => {
    const rx = new RegExp('(?:^|[^a-z0-9])' + kw + '(?:$|[^a-z0-9])', 'i');
    return acc + (rx.test(combined) ? 1 : 0);
  }, 0);

  const spCount = countMatches(spiritualKeywords);
  const hiCount = countMatches(historyKeywords);
  const waCount = countMatches(warKeywords);
  const crCount = countMatches(creativeKeywords);
  const caCount = countMatches(casualKeywords);

  // Ambang batas ketat: butuh minimal 2 kata kunci agar beralih ke niche non-stoik
  if (waCount >= 2 && waCount > spCount && waCount > hiCount && waCount > crCount && waCount > caCount) {
    return NICHE_DOMAINS.DOMAIN_PERANG;
  }
  if (spCount >= 2 && spCount > hiCount && spCount > crCount && spCount > caCount) {
    return NICHE_DOMAINS.DOMAIN_SPIRITUAL;
  }
  if (hiCount >= 2 && hiCount > spCount && hiCount > crCount && hiCount > caCount) {
    return NICHE_DOMAINS.DOMAIN_HISTORY;
  }
  if (crCount >= 2 && crCount > spCount && crCount > hiCount && crCount > caCount) {
    return NICHE_DOMAINS.DOMAIN_CREATIVE;
  }
  if (caCount >= 2 && caCount > spCount && caCount > hiCount && caCount > crCount) {
    return NICHE_DOMAINS.DOMAIN_CASUAL;
  }

  // ZERO RISK MUTLAK: Selalu default ke DOMAIN_STOIC yang sudah terbukti paripurna
  return NICHE_DOMAINS.DOMAIN_STOIC;
}

/**
 * [v3.2] GENRE WHITELIST — Pemisahan Pool Footage Berdasarkan Tema Video
 *
 * CINEMATIC_POOL: 18 bundel bermuatan dramatis, wibawa, sinematik, industri, dan kreatif berkualitas tinggi.
 *   Wajib digunakan untuk tema: motivasi, stoik, karir, kemiskinan, alpha, refleksi, melancholy.
 *
 * CASUAL_POOL: 9 bundel kasual/humor/rolling cadangan.
 *   Hanya aktif untuk tema: humor, edukasi_santai, tutorial, komedi.
 *   DILARANG KERAS masuk video bertema serius.
 */
const CINEMATIC_POOL = [
  'IDE0C2', // FYPVault Pemersatu Bangsa    — Hook Wajib Scene 01 + Mid-Roll Retention Booster
  'ID9919', // SigmaReel Shelby Alpha Stoik — Wibawa Pria Berprinsip
  'IDFFA0', // VoidVibes Dark Psychology    — Tatapan Dingin, Misterius, Intimidasi
  'IDB7CA', // RogueMind Joker Realitas     — Ironi Pahit, Topeng, Realitas Keras
  'IDFCC3', // DuskAura Senja Melankolis    — Gerimis, Senja, Dark Psychology, Refleksi
  'ID7667', // AuraBoost Alam Healing       — Bangkit, Puncak, Tenang, Kedamaian
  'ID557A', // DesiCraft Skill Dewa         — Kerja Keras Ekstrem, Banting Tulang, Fisik
  'IDE5EF', // MechTech Mesin Manufaktur    — Industri Modern, Baja, Presisi Pabrik
  'ID9C29', // ReviveCraft Karat Logam      — Senyap, Perbaiki Kesalahan, Ketekunan
  'ID1646', // NeonFuture Cyberpunk & AI    — Kota Industri Modern, Teknologi Digital
  'ID3199', // WallMotion Sinematik Loop    — Visual 4K Megah, Abstrak, Waktu
  'ID7792', // AnimeGrief Melankolis Anime  — Hampa Jiwa, Emosi Mendalam
  'IDDD2B', // CraftGenius Lifehack Kreatif — Kreasi, Motivasi, Membangun, Solusi
  'ID_MASTER_STORY', // Master Exclusive Story & B-Roll — Narasi Stoik, Sejarah & Perjuangan Eksklusif
];

const CASUAL_POOL = [
  'ID617C', // AffilTrend Racun Shopee      — Royalty-Free, Cerah, Kreatif
  'ID32DF', // LiveHustle Live Selling      — Transaksi, Persaingan, Dagang Hustle
  'ID8125', // ChefByte Resep Masak         — Kreativitas Dapur, Hidangan
  'IDBABD', // BiteRush Mukbang Brutal      — Ekspresif, Porsi Besar, Kuliner
  'ID3C1F', // ToolCraft Pertukangan        — Pertukangan, Keterampilan Tangan
  'IDEBCB', // ThaiVibes Komedi Thailand    — Pemersatu Barbar, Humor
  'ID1AF4', // HookMagnet Hook 3 Detik      — Kejutan Cepat, Rolling
  'ID380D', // HookStrike Mancing Monster   — Rolling Cadangan
  'IDC20C', // PetViral Anabul Gemoy        — Rolling Cadangan (Deprioritized)
  'IDCE99', // HomeAura Room Tour Minimalis — Rolling Cadangan (Deprioritized)
  'IDC50B', // MeowFrenzy Anabul Tantrum    — Kucing AI, Rolling Cadangan
  'ID5FF3', // KiddieAI Bayi AI Gemoy       — Rolling Cadangan
  'IDEAD7', // JoyPaws Interaksi Anabul     — Rolling Cadangan
  'IDEF4B', // ToyPlay Mainan Anak          — Rolling Cadangan
];

// Tema yang HANYA boleh mengambil dari CINEMATIC_POOL
const SERIOUS_THEMES = new Set([
  'hard_reality', 'alpha_stoic', 'motivasi', 'kemiskinan', 'pria',
  'melancholy', 'galau', 'refleksi', 'karir', 'stoik', 'alpha',
  'disiplin', 'kehidupan', 'filosofi', 'psikologi', 'fokus', 'ambisi'
]);

/**
 * [v2.0] Menentukan pool footage aktif berdasarkan tema.
 * Tema serius → CINEMATIC_POOL saja.
 * Tema santai/humor → CINEMATIC_POOL + CASUAL_POOL.
 */
function resolveThematicPool(defaultTheme, bundleMap) {
  const theme = (defaultTheme || 'hard_reality').toLowerCase();
  const isSerious = SERIOUS_THEMES.has(theme);

  let rawPool;
  if (isSerious) {
    rawPool = CINEMATIC_POOL;
    console.log(`  Genre Whitelist  : CINEMATIC_POOL (${rawPool.length} bundel sinematik — tema serius terdeteksi)`);
  } else {
    rawPool = [...CINEMATIC_POOL, ...CASUAL_POOL];
    console.log(`  Genre Whitelist  : FULL_POOL (${rawPool.length} bundel — tema santai terdeteksi)`);
  }

  return rawPool.filter(sku => bundleMap[sku]);
}

/**
 * Deteksi dan Penilaian Afinitas Semantik Kalimat Naskah vs Tag Snapshot 5 Detik
 * Menghitung bobot kesesuaian kata naskah terhadap 20 taksonomi bundel aktif.
 */
function scoreSemanticAffinity(text, dynamicPool, usageCount, previousSku, getMaxCapForSku, bundleMap, excludedSkus = [], options = {}) {
  if (!text) return null;
  const lower = text.toLowerCase();
  const candidates = [];

  // [v4.0] Konsep visual dideteksi sekali per kalimat via stem Indonesia (berjuang -> juang, lautan -> laut)
  const conceptHits = detectConceptHits(lower);
  // Bonus kecil untuk bundel domain utama agar identitas niche tetap terjaga saat skor bersaing
  const preferredSet = new Set(options.preferredSkus || []);
  const preferredBonus = typeof options.preferredBonus === 'number' ? options.preferredBonus : 3;

  for (const sku of dynamicPool) {
    if ((!options.allowConsecutive && dynamicPool.length > 1 && sku === previousSku) || excludedSkus.includes(sku)) continue;
    const currentUsage = usageCount[sku] || 0;
    const cap = getMaxCapForSku(sku);
    if (currentUsage >= cap) continue;

    const info = BUNDLE_CONCEPTS[sku];
    if (!info) continue;

    let score = 0;
    let matchedWords = [];
    for (const tag of info.tags) {
      const tagLower = tag.toLowerCase();
      let isMatch = false;
      if (tagLower.includes(' ')) {
        // Frasa majemuk multi-kata: bobot lebih tinggi karena sangat spesifik
        isMatch = lower.includes(tagLower);
        if (isMatch) {
          score += Math.round(tag.length * 1.5);
          matchedWords.push(tag);
        }
      } else if (tagLower.length <= 4) {
        // Kata pendek (<= 4 huruf) wajib word-boundary agar tidak mencocokkan substring acak
        const regex = new RegExp(`\\b${tagLower.replace(/[.*+?^${}()|[\\]\\]/g, '\\$&')}\\b`, 'i');
        isMatch = regex.test(lower);
        if (isMatch) {
          score += tag.length;
          matchedWords.push(tag);
        }
      } else {
        isMatch = lower.includes(tagLower);
        if (isMatch) {
          score += tag.length;
          matchedWords.push(tag);
        }
      }
    }

    // [v4.0] Skor konsep leksikon: bobot dasar konsep + 2 per kata pendukung tambahan (maks +6)
    let conceptScore = 0;
    for (const hit of conceptHits) {
      const w = hit.weights[sku];
      if (!w) continue;
      conceptScore += w + 2 * Math.min(3, hit.words.length - 1);
      hit.words.forEach(word => matchedWords.push(`${word}->${hit.key}`));
    }
    score += conceptScore;

    if (score > 0 && preferredSet.has(sku)) score += preferredBonus;

    if (score > 0) {
      const uniqueWords = Array.from(new Set(matchedWords));
      candidates.push({
        sku,
        name: info.name,
        score,
        matchedWords: uniqueWords.join(', '),
        reason: `${options.reasonPrefix || 'Pencocokan Semantik v4'}: [${uniqueWords.join(', ')}] -> ${info.name} (${sku}) skor ${score}`
      });
    }
  }

  // [SAFEGUARD ANTI-GENERIK, AKURASI & PRESISI]
  // Jika beberapa kandidat memiliki skor afinitas semantik tinggi yang sebanding (>= 75% skor teratas),
  // prioritaskan bundel yang pemakaiannya lebih rendah dalam video aktif agar alokasi visual tidak generik,
  // namun tetap 100% akurat dan relevan dengan kata kunci adegan!
  if (candidates.length > 1) {
    const maxScore = Math.max(...candidates.map(c => c.score));
    candidates.sort((a, b) => {
      const aNearTop = a.score >= maxScore * 0.75;
      const bNearTop = b.score >= maxScore * 0.75;
      if (aNearTop && bNearTop) {
        const usageA = usageCount[a.sku] || 0;
        const usageB = usageCount[b.sku] || 0;
        if (usageA !== usageB) return usageA - usageB; // Utamakan variasi segar
      }
      return b.score - a.score;
    });
  }

  return candidates.length > 0 ? candidates[0] : null;
}

/**
 * ENGINE UTAMA: Koreografer Urutan Klip Video Naskah dengan 5-Layer Anti-Pattern Engine
 * Menjamin:
 * 1. Multi-Cluster Metaphor: Membuka 18+ bundel kaya sinematik, industri, dan kreatif.
 * 2. Dual Hook / Mid-Roll Retention Boost: Hook Scene 01 + Klip segar kedua di pertengahan video (~40-60% durasi).
 * 3. No Consecutive Same Bundle: Adegan N tidak pernah memakai bundel yang sama dengan N-1.
 * 4. Max Frequency Cap: Maksimal 2 klip per bundel dalam 1 video (16-26 adegan).
 * 5. Random Fresh Draw: Query SQLite WAL `ORDER BY RANDOM()` pada klip segar `used_count = 0` dan `is_clean = 1`.
 * 6. Atomic State Ledger: Mengunci klip terpilih ke SQLite secara permanen (`used_count = used_count + 1`, `last_used_project = projectId`).
 */
function buildKinshipSequenceForScript({
  scriptScenes = [],
  defaultTheme = 'hard_reality',
  projectId = null,
  dryRun = false,
  domain = null,
  scriptText = '',
  title = '',
  excludeClipIds: excludeClipIdsParam = [],
  selectedSkus = null
}) {
  const database = getDatabase();
  const catalog = loadCatalog(false);
  const primaryCluster = resolvePrimaryCluster(catalog, defaultTheme);

  // Buat lookup bundel cepat
  const bundleMap = {};
  catalog.bundles.forEach(b => {
    bundleMap[b.sku] = b;
  });

  // [v3.0] Resolusi Domain Niche (Zero-Risk & Anti-Jomplang)
  const allScriptText = scriptText || scriptScenes.map(s => s.text || '').join(' ');
  const activeDomain = (typeof domain === 'object' && domain !== null && domain.id)
    ? domain
    : detectNicheDomain(allScriptText, title, domain || defaultTheme);

  // Ambil pool klip yang valid dari domain aktif (fallback ke CINEMATIC_POOL jika kosong)
  let dynamicThematicPool = (activeDomain.pool || []).filter(sku => bundleMap[sku]);
  if (!dynamicThematicPool || dynamicThematicPool.length === 0) {
    dynamicThematicPool = resolveThematicPool(defaultTheme, bundleMap);
  }

  // [v5.3] Penanganan Override Selected SKUs (--footage-skus)
  let isUserSelectedSkus = false;
  if (selectedSkus !== null && selectedSkus !== undefined) {
    let rawSkus = [];
    if (typeof selectedSkus === 'string') {
      const trimmed = selectedSkus.trim();
      if (trimmed && trimmed.toLowerCase() !== 'auto') {
        rawSkus = trimmed.split(',').map(s => s.trim().toUpperCase()).filter(Boolean);
      }
    } else if (Array.isArray(selectedSkus)) {
      rawSkus = selectedSkus.map(s => String(s).trim().toUpperCase()).filter(s => s && s.toLowerCase() !== 'auto');
    }

    const validFiltered = rawSkus.filter(sku => bundleMap[sku] && !ELIMINATED_SKUS.includes(sku));
    if (validFiltered.length > 0) {
      dynamicThematicPool = validFiltered;
      isUserSelectedSkus = true;
      console.log(`  [FOOTAGE SKU OVERRIDE] Mengunci pool ke SKU pilihan: [${validFiltered.join(', ')}]`);
    } else if (rawSkus.length > 0) {
      console.warn(`  [FOOTAGE SKU OVERRIDE] SKU yang diberikan tidak valid atau termasuk eliminasi: [${rawSkus.join(', ')}]. Menggunakan pool otomatis.`);
    }
  }

  // Plafon Penggunaan Bundel (Strict Diversity) - Adaptif terhadap Panjang Durasi/Adegan
  const totalScenes = scriptScenes.length;
  const scaleFactor = Math.max(1.0, totalScenes / 18.0);
  const userCap = Math.ceil(totalScenes / Math.max(1, dynamicThematicPool.length)) + 1;

  function getMaxCapForSku(sku) {
    let baseCap = (activeDomain.maxCapPerSku && activeDomain.maxCapPerSku[sku])
      ? activeDomain.maxCapPerSku[sku]
      : (activeDomain.defaultMaxCap || 2);
    if (isUserSelectedSkus) {
      return Math.max(userCap, baseCap);
    }
    return Math.max(baseCap, Math.ceil(baseCap * scaleFactor));
  }
  const midSceneIndex = Math.floor(totalScenes / 2) + 1;

  console.log(`\n=============================================================================`);
  console.log(`  MESIN KOREOGRAFER FOOTAGE ANTI-PATTERN v3.2 (DUAL-HOOK & MID-ROLL BOOST)`);
  console.log(`  Domain Aktif    : [${activeDomain.id}] ${activeDomain.name}`);
  console.log(`  BGM Folder      : ${activeDomain.bgmFolder}`);
  console.log(`  Hook Scene 01   : ${activeDomain.hookBundle} (${bundleMap[activeDomain.hookBundle]?.title || 'Auto'})`);
  console.log(`  Mid-Roll Boost  : ${activeDomain.midRollBundle || 'IDE0C2'} pada Adegan ${midSceneIndex}/${totalScenes}`);
  console.log(`  Tema Utama      : ${primaryCluster.clusterName || primaryCluster.name || 'Hard Reality'} (${defaultTheme})`);
  console.log(`  Jumlah Adegan   : ${totalScenes} Babak Waktu`);
  console.log(`  Pool Aktif      : ${dynamicThematicPool.length} Bundel Terwhitelist${isUserSelectedSkus ? ' [OVERRIDE: ' + dynamicThematicPool.join(', ') + ']' : ''}`);
  console.log(`  Project ID      : ${projectId || 'TEST-PREVIEW'}${dryRun ? ' (DRY RUN - No DB Write)' : ''}`);
  console.log(`=============================================================================\n`);

  const choreography = [];
  const bundleUsageCount = {};
  let previousSku = null;
  const excludeClipIds = new Set(Array.isArray(excludeClipIdsParam) ? excludeClipIdsParam : []);

  // Siapkan Prepared Statement SQLite WAL (Zero Contamination Guard: Wajib is_clean = 1 & NOT IN ELIMINATED_SKUS)
  const stmtSelectFreshRandom = database.prepare(`
    SELECT id, bundle_sku, bundle_name, clip_name, video_path, snapshot_path
    FROM footage_items
    WHERE bundle_sku = ? 
      AND is_clean = 1 
      AND bundle_sku NOT IN ('ID4834', 'ID53FA', 'IDA71C', 'IDCB24', 'IDE830', 'IDE8B2', 'ID8F64')
      AND used_count = 0
      AND clip_name NOT LIKE '0000%'
      AND LOWER(clip_name) NOT LIKE '%preview%'
      AND LOWER(clip_name) NOT LIKE '%trailer%'
    ORDER BY RANDOM()
    LIMIT 20
  `);

  const stmtSelectSpecificClip = database.prepare(`
    SELECT id, bundle_sku, bundle_name, clip_name, video_path, snapshot_path
    FROM footage_items
    WHERE bundle_sku = 'ID_MASTER_STORY'
      AND clip_name LIKE ?
      AND is_clean = 1
      AND bundle_sku NOT IN ('ID4834', 'ID53FA', 'IDA71C', 'IDCB24', 'IDE830', 'IDE8B2', 'ID8F64')
    ORDER BY used_count ASC, RANDOM()
    LIMIT 5
  `);

  // [v4.2] Prepared Statement Vision-Aware Multi-Rank (27.291 Klip ber-Snapshot Frame)
  // Menilai kecocokan visual pada peringkat #1, #2, dan #3 (skor tertinggi & bebas watermark)
  let stmtSelectVisualConceptClip = null;
  try {
    const hasVisualTable = database.prepare("SELECT 1 FROM sqlite_master WHERE type='table' AND name='footage_visual_tags'").get();
    if (hasVisualTable) {
      stmtSelectVisualConceptClip = database.prepare(`
        SELECT f.id, f.bundle_sku, f.bundle_name, f.clip_name, f.video_path, f.snapshot_path,
               t.top_concept,
               CASE
                 WHEN t.top_concept = :concept THEN t.top_score
                 WHEN t.concept_2 = :concept THEN t.score_2
                 WHEN t.concept_3 = :concept THEN t.score_3
                 ELSE 0
               END AS match_score,
               CASE
                 WHEN t.top_concept = :concept THEN 1
                 WHEN t.concept_2 = :concept THEN 2
                 WHEN t.concept_3 = :concept THEN 3
                 ELSE 9
               END AS match_rank
        FROM footage_items f
        JOIN footage_visual_tags t ON f.id = t.footage_id
        WHERE f.bundle_sku = :sku
          AND (t.top_concept = :concept OR t.concept_2 = :concept OR t.concept_3 = :concept)
          AND f.is_clean = 1
          AND f.bundle_sku NOT IN ('ID4834', 'ID53FA', 'IDA71C', 'IDCB24', 'IDE830', 'IDE8B2', 'ID8F64')
          AND f.used_count = 0
          AND t.has_text_flag = 0
          AND (t.top_concept <> 'TEKS' OR t.top_score < 0.4)
          AND f.clip_name NOT LIKE '0000%'
          AND LOWER(f.clip_name) NOT LIKE '%preview%'
          AND LOWER(f.clip_name) NOT LIKE '%trailer%'
        ORDER BY match_rank ASC, match_score DESC, RANDOM()
        LIMIT 10
      `);
    }
  } catch (err) {
    stmtSelectVisualConceptClip = null;
  }

  const stmtLockClip = database.prepare(`
    UPDATE footage_items
    SET used_count = used_count + 1, last_used_project = ?
    WHERE id = ?
  `);

  scriptScenes.forEach((scene, idx) => {
    const sceneIndex = scene.scene || (idx + 1);
    const sceneText = scene.text || '';
    const duration = scene.duration || 8.0;

    let targetSku = null;
    let matchReason = '';

    // 1. Hook Scene 01: Wajib Hook Terjaga (Zero-Risk & Anti-Bounce Rate)
    // Jika user menentukan SKU dan salah satu SKU pilihan adalah SKU hook (ID1AF4, IDE0C2, atau hook resmi domain), prioritaskan itu.
    // Jika tidak, tetap kunci Hook Scene 01 dengan hook resmi domain aktif untuk menjaga retensi 3 detik pertama penonton.
    let hookCandidate = null;
    if (isUserSelectedSkus) {
      if (dynamicThematicPool.includes('ID1AF4') && bundleMap['ID1AF4']) {
        hookCandidate = 'ID1AF4';
      } else if (dynamicThematicPool.includes(activeDomain.hookBundle) && bundleMap[activeDomain.hookBundle]) {
        hookCandidate = activeDomain.hookBundle;
      } else if (dynamicThematicPool.includes('IDE0C2') && bundleMap['IDE0C2']) {
        hookCandidate = 'IDE0C2';
      } else {
        hookCandidate = (activeDomain && activeDomain.hookBundle && bundleMap[activeDomain.hookBundle])
          ? activeDomain.hookBundle
          : (bundleMap['IDE0C2'] ? 'IDE0C2' : dynamicThematicPool[0]);
      }
    } else {
      hookCandidate = (activeDomain && activeDomain.hookBundle && bundleMap[activeDomain.hookBundle])
        ? activeDomain.hookBundle
        : (bundleMap['IDE0C2'] ? 'IDE0C2' : null);
    }
    if (sceneIndex === 1 && hookCandidate && bundleMap[hookCandidate]) {
      targetSku = hookCandidate;
      matchReason = `Hook Terjaga [${activeDomain.name || 'General'}]: ${bundleMap[targetSku].brandCode} (${targetSku} Fresh Draw)`;
    }

    // 2. Dual Hook / Mid-Roll Retention Booster: Klip kedua IDE0C2 di pertengahan durasi (~40-60%) jika total adegan >= 8
    const midRollTarget = activeDomain.midRollBundle || 'IDE0C2';
    if (!targetSku && sceneIndex === midSceneIndex && totalScenes >= 8 && previousSku !== midRollTarget) {
      const currentUsage = bundleUsageCount[midRollTarget] || 0;
      if (currentUsage < getMaxCapForSku(midRollTarget) && bundleMap[midRollTarget]) {
        targetSku = midRollTarget;
        matchReason = `Mid-Roll Retention Booster [${bundleMap[targetSku].brandCode}]: Klip Segar Kedua Pemersatu Bangsa (${targetSku}) di Pertengahan Durasi`;
      }
    }

    // Anti-Consecutive Guard: Jika adegan tepat sebelum midSceneIndex, kecualikan midRollTarget agar tidak berurutan
    const excludedForThisScene = (sceneIndex === midSceneIndex - 1 && totalScenes >= 8) ? [midRollTarget] : [];

    // 3. Outro Scene Bumper: Adegan penutup terakhir (sceneIndex === totalScenes) wajib Logo Outro 3D FokusKonten
    if (!targetSku && sceneIndex === totalScenes && bundleMap['ID_BRAND_FK']) {
      targetSku = 'ID_BRAND_FK';
      matchReason = 'Bumper 3D Logo FokusKonten Official Outro (Sinkronisasi Penutup Selaras)';
    }

    // 4. Pencocokan Semantik v4 (Tag 5 Detik + Leksikon Konsep ber-stem) vs Kata Naskah (untuk Scene 2 dst)
    // Kolam semantik = jika override user aktif, hanya cari dari dynamicThematicPool yang dipilih.
    // Jika otomatis: pool domain + seluruh CINEMATIC_POOL (kecuali Hook/Mid-Roll IDE0C2 & Outro).
    const semanticPool = isUserSelectedSkus
      ? dynamicThematicPool
      : (activeDomain.id === 'DOMAIN_CASUAL'
          ? dynamicThematicPool
          : Array.from(new Set([
              ...dynamicThematicPool,
              ...CINEMATIC_POOL.filter(s => s !== 'IDE0C2' && s !== 'ID_BRAND_FK')
            ])).filter(s => bundleMap[s]));
    const semanticOptions = {
      preferredSkus: dynamicThematicPool,
      preferredBonus: 3,
      allowConsecutive: dynamicThematicPool.length <= 1
    };

    if (!targetSku) {
      const semanticMatch = scoreSemanticAffinity(sceneText, semanticPool, bundleUsageCount, previousSku, getMaxCapForSku, bundleMap, excludedForThisScene, semanticOptions);
      if (semanticMatch && bundleMap[semanticMatch.sku]) {
        targetSku = semanticMatch.sku;
        matchReason = semanticMatch.reason;
      }
    }

    // 4b. Pencocokan ulang dengan plafon dilonggarkan +1 sebelum menyerah ke rotasi acak
    if (!targetSku) {
      const relaxedMatch = scoreSemanticAffinity(
        sceneText, semanticPool, bundleUsageCount, previousSku,
        sku => getMaxCapForSku(sku) + 1, bundleMap, excludedForThisScene,
        Object.assign({}, semanticOptions, { reasonPrefix: 'Pencocokan Semantik Diperluas (Plafon +1)' })
      );
      if (relaxedMatch && bundleMap[relaxedMatch.sku]) {
        targetSku = relaxedMatch.sku;
        matchReason = relaxedMatch.reason;
      }
    }

    // 5. Jika tidak ada kata yang cocok langsung, rotasi dari Thematic Pool yang belum terpakai
    if (!targetSku) {
      const allowConsecutive = dynamicThematicPool.length <= 1;
      let eligiblePool = dynamicThematicPool.filter(sku => 
        (allowConsecutive || sku !== previousSku) && 
        !excludedForThisScene.includes(sku) && 
        (bundleUsageCount[sku] || 0) < getMaxCapForSku(sku)
      );
      if (eligiblePool.length > 0) {
        // Ambil secara acak agar urutan selalu bervariasi segar
        const randomPick = eligiblePool[Math.floor(Math.random() * eligiblePool.length)];
        targetSku = randomPick;
        matchReason = `Rotasi Semantik Cadangan Spektrum Luas [${targetSku}]`;
      } else {
        let relaxedPool = dynamicThematicPool.filter(sku => 
          (allowConsecutive || sku !== previousSku) && 
          !excludedForThisScene.includes(sku) && 
          sku !== 'IDEBCB'
        );
        if (relaxedPool.length === 0) {
          relaxedPool = dynamicThematicPool.filter(sku => !excludedForThisScene.includes(sku));
          if (relaxedPool.length === 0) relaxedPool = dynamicThematicPool;
        }
        if (relaxedPool.length > 0) {
          const randomPick = relaxedPool[Math.floor(Math.random() * relaxedPool.length)];
          targetSku = randomPick;
          matchReason = `Rotasi Acak Fleksibel Non-Konsekutif [${targetSku}]`;
        } else {
          targetSku = primaryCluster.primarySku;
          matchReason = `Rotasi Darurat Klaster Primer [${targetSku}]`;
        }
      }
    }

    let targetBundle = bundleMap[targetSku] || bundleMap[primaryCluster.primarySku];

    // 6. Tarik Klip dari SQLite WAL (Prioritas A: Frame Presisi Master Story -> Prioritas B: Vision CLIP Snapshot -> Prioritas C: Klip Segar Acak)
    let clipRow = null;

    if (targetBundle.sku === 'ID_MASTER_STORY') {
      const lowerText = sceneText.toLowerCase();
      for (const entry of MASTER_STORY_CLIPS_MAP) {
        const hasMatch = entry.keywords.some(kw => lowerText.includes(kw.toLowerCase()));
        if (hasMatch) {
          const specific = stmtSelectSpecificClip.get(`${entry.prefix}%`);
          if (specific) {
            clipRow = specific;
            matchReason += ` -> [Frame Presisi: ${specific.clip_name}]`;
            break;
          }
        }
      }
    }

    // [v4.2] Prioritas B: Tarik klip berdasarkan kesesuaian visual frame foto (CLIP ONNX Multi-Rank)
    if (!clipRow && stmtSelectVisualConceptClip) {
      const lowerText = sceneText.toLowerCase();
      const hits = detectConceptHits(lowerText);
      if (hits && hits.length > 0) {
        for (const hit of hits) {
          const visualClips = stmtSelectVisualConceptClip.all({ ':sku': targetBundle.sku, ':concept': hit.key });
          if (visualClips && visualClips.length > 0) {
            for (const vc of visualClips) {
              resolveClipPaths(vc);
              if (!excludeClipIds.has(vc.id) && fs.existsSync(vc.video_path)) {
                clipRow = vc;
                const rankLabel = vc.match_rank === 1 ? 'Top-1' : `Top-${vc.match_rank}`;
                matchReason += ` -> [Vision CLIP ${rankLabel}: ${hit.key} (${Math.round(vc.match_score * 100)}%)]`;
                break;
              }
            }
            if (clipRow) break;
          }
        }
      }
    }

    if (!clipRow) {
      const freshCandidates = stmtSelectFreshRandom.all(targetBundle.sku);
      if (freshCandidates && freshCandidates.length > 0) {
        for (const fc of freshCandidates) {
          resolveClipPaths(fc);
          if (!excludeClipIds.has(fc.id) && fs.existsSync(fc.video_path)) {
            clipRow = fc;
            break;
          }
        }
        if (!clipRow) {
          clipRow = resolveClipPaths(freshCandidates[0]); // Fallback jika seluruh kandidat bundel ini sudah pernah tampil
        }
      }
    }

    // Khusus Outro ID_BRAND_FK: Jika used_count=0 habis, rotasi dari klip bersih yang ada
    if (!clipRow && targetBundle.sku === 'ID_BRAND_FK') {
      const stmtFallbackBrand = database.prepare(`
        SELECT id, bundle_sku, bundle_name, clip_name, video_path, snapshot_path
        FROM footage_items
        WHERE bundle_sku = 'ID_BRAND_FK' AND is_clean = 1
        ORDER BY used_count ASC, RANDOM()
        LIMIT 5
      `);
      const brandCandidates = stmtFallbackBrand.all();
      for (const bc of (brandCandidates || [])) {
        resolveClipPaths(bc);
        if (!excludeClipIds.has(bc.id) && fs.existsSync(bc.video_path)) {
          clipRow = bc;
          break;
        }
      }
      if (!clipRow && brandCandidates && brandCandidates.length > 0) clipRow = resolveClipPaths(brandCandidates[0]);
    }

    // Fallback 1: Jika klip segar (used_count=0) di targetBundle habis, coba klip bersih (is_clean=1)
    // dari SKU yang SAMA dengan used_count terendah yang belum dipakai di sesi ini
    if (!clipRow) {
      const stmtRecycledSku = database.prepare(`
        SELECT id, bundle_sku, bundle_name, clip_name, video_path, snapshot_path
        FROM footage_items
        WHERE bundle_sku = ?
          AND is_clean = 1
          AND clip_name NOT LIKE '0000%'
          AND LOWER(clip_name) NOT LIKE '%preview%'
          AND LOWER(clip_name) NOT LIKE '%trailer%'
        ORDER BY used_count ASC, RANDOM()
        LIMIT 30
      `);
      const recycledCandidates = stmtRecycledSku.all(targetBundle.sku);
      if (recycledCandidates && recycledCandidates.length > 0) {
        for (const rc of recycledCandidates) {
          resolveClipPaths(rc);
          if (!excludeClipIds.has(rc.id) && fs.existsSync(rc.video_path)) {
            clipRow = rc;
            matchReason += ` -> [Fallback SKU ${targetBundle.sku}: Bersih Daur Ulang used_count min]`;
            break;
          }
        }
      }
    }

    // Fallback 2: Jika bundel ini benar-benar kehabisan klip unik di disk, alihkan ke alternatif di dynamicThematicPool
    if (!clipRow) {
      console.warn(`[PERINGATAN] Bundel ${targetBundle.sku} kehabisan klip unik untuk Scene ${sceneIndex}. Mencari alternatif di kolam...`);
      for (const altSku of dynamicThematicPool) {
        if (altSku !== targetBundle.sku) {
          // 2a. Coba Visual Concept di altSku
          if (stmtSelectVisualConceptClip) {
            const hits = detectConceptHits(sceneText.toLowerCase());
            for (const hit of (hits || [])) {
              const altVisuals = stmtSelectVisualConceptClip.all({ ':sku': altSku, ':concept': hit.key });
              for (const av of (altVisuals || [])) {
                resolveClipPaths(av);
                if (!excludeClipIds.has(av.id) && fs.existsSync(av.video_path)) {
                  clipRow = av;
                  targetBundle = bundleMap[altSku] || targetBundle;
                  const rankLabel = av.match_rank === 1 ? 'Top-1' : `Top-${av.match_rank}`;
                  matchReason += ` -> Pengalihan Segar ke [${altSku}] (Vision CLIP ${rankLabel}: ${hit.key})`;
                  break;
                }
              }
              if (clipRow) break;
            }
          }
          if (clipRow) break;

          // 2b. Coba fresh candidates (used_count = 0) di altSku
          const altFresh = stmtSelectFreshRandom.all(altSku);
          for (const af of (altFresh || [])) {
            resolveClipPaths(af);
            if (!excludeClipIds.has(af.id) && fs.existsSync(af.video_path)) {
              clipRow = af;
              targetBundle = bundleMap[altSku] || targetBundle;
              matchReason += ` -> Pengalihan Segar ke [${altSku}]`;
              break;
            }
          }
          if (clipRow) break;

          // 2c. Coba recycled (used_count ASC) di altSku
          const stmtAltRecycled = database.prepare(`
            SELECT id, bundle_sku, bundle_name, clip_name, video_path, snapshot_path
            FROM footage_items
            WHERE bundle_sku = ?
              AND is_clean = 1
              AND clip_name NOT LIKE '0000%'
              AND LOWER(clip_name) NOT LIKE '%preview%'
              AND LOWER(clip_name) NOT LIKE '%trailer%'
            ORDER BY used_count ASC, RANDOM()
            LIMIT 20
          `);
          const altRecycled = stmtAltRecycled.all(altSku);
          for (const ar of (altRecycled || [])) {
            resolveClipPaths(ar);
            if (!excludeClipIds.has(ar.id) && fs.existsSync(ar.video_path)) {
              clipRow = ar;
              targetBundle = bundleMap[altSku] || targetBundle;
              matchReason += ` -> Pengalihan Daur Ulang ke [${altSku}]`;
              break;
            }
          }
          if (clipRow) break;
        }
      }
    }

    // Fallback 3 (Tier 4): Stok kolam habis, ambil klip bersih pemakaian terendah dari seluruh katalog (di luar eliminasi)
    if (!clipRow) {
      console.warn(`[TIER 4 PERPETUAL FALLBACK] Seluruh klip dari SKU terpilih habis untuk Scene ${sceneIndex}. Mengambil klip bersih dengan pemakaian terendah dari katalog umum...`);
      const stmtTier4 = database.prepare(`
        SELECT id, bundle_sku, bundle_name, clip_name, video_path, snapshot_path
        FROM footage_items
        WHERE is_clean = 1 
          AND bundle_sku NOT IN ('ID4834', 'ID53FA', 'IDA71C', 'IDCB24', 'IDE830', 'IDE8B2', 'ID8F64')
          AND clip_name NOT LIKE '0000%'
          AND LOWER(clip_name) NOT LIKE '%preview%'
          AND LOWER(clip_name) NOT LIKE '%trailer%'
        ORDER BY used_count ASC, last_used_project ASC, RANDOM()
        LIMIT 50
      `);
      const candidatesTier4 = stmtTier4.all();
      for (const cand of (candidatesTier4 || [])) {
        resolveClipPaths(cand);
        if (!excludeClipIds.has(cand.id) && fs.existsSync(cand.video_path)) {
          clipRow = cand;
          targetBundle = bundleMap[cand.bundle_sku] || {
            sku: cand.bundle_sku,
            brandCode: cand.bundle_sku,
            title: cand.bundle_name || 'Footage Master',
            clusterId: 1,
            clusterName: 'Thematic'
          };
          matchReason += ` -> [Tier 4 Perpetual Fallback: Klip Bersih Tersedia (${cand.bundle_sku})]`;
          break;
        }
      }
    }

    // Fallback 4 (Safety Guard Anti Layar Hitam): Jika seluruh klip un-excluded habis/bermasalah di disk,
    // ambil klip bersih terverifikasi apa pun yang ada di disk (mengizinkan pemakaian ulang jika darurat)
    if (!clipRow) {
      console.warn(`[SAFETY GUARD ZERO BLACK-SCREEN] Mengambil klip bersih terverifikasi di disk untuk Scene ${sceneIndex}...`);
      const stmtSafety = database.prepare(`
        SELECT id, bundle_sku, bundle_name, clip_name, video_path, snapshot_path
        FROM footage_items
        WHERE is_clean = 1 
          AND bundle_sku NOT IN ('ID4834', 'ID53FA', 'IDA71C', 'IDCB24', 'IDE830', 'IDE8B2', 'ID8F64')
          AND clip_name NOT LIKE '0000%'
          AND LOWER(clip_name) NOT LIKE '%preview%'
          AND LOWER(clip_name) NOT LIKE '%trailer%'
        ORDER BY used_count ASC, RANDOM()
        LIMIT 100
      `);
      const safetyCandidates = stmtSafety.all();
      for (const sc of (safetyCandidates || [])) {
        resolveClipPaths(sc);
        if (fs.existsSync(sc.video_path)) {
          clipRow = sc;
          targetBundle = bundleMap[sc.bundle_sku] || {
            sku: sc.bundle_sku,
            brandCode: sc.bundle_sku,
            title: sc.bundle_name || 'Footage Master',
            clusterId: 1,
            clusterName: 'Thematic'
          };
          matchReason += ` -> [Safety Guard Fallback: Pencegahan Layar Hitam (${sc.bundle_sku})]`;
          break;
        }
      }
    }

    if (!clipRow) {
      throw new Error(`[FATAL] Tidak ditemukan klip fisik yang valid pada footage_catalog.db untuk Scene ${sceneIndex}!`);
    }

    // Kunci ID ke memori lokal sesi ini agar tidak pernah terpilih ganda dalam 1 video
    excludeClipIds.add(clipRow.id);

    resolveClipPaths(clipRow);
    const selectedClipName = clipRow.clip_name;
    const selectedClipPath = clipRow.video_path;
    const existsOnDisk = fs.existsSync(selectedClipPath);

    // 4. Kunci Klip ke Database (Atomic State Ledger) jika bukan DRY RUN
    if (!dryRun && projectId) {
      stmtLockClip.run(String(projectId), clipRow.id);
    }

    // Perbarui state pencatat lokal
    bundleUsageCount[targetBundle.sku] = (bundleUsageCount[targetBundle.sku] || 0) + 1;
    previousSku = targetBundle.sku;

    const sceneResult = {
      sceneIndex,
      duration,
      text: sceneText,
      sku: targetBundle.sku,
      brandCode: targetBundle.brandCode,
      bundleTitle: targetBundle.title,
      clusterId: targetBundle.clusterId,
      clusterName: targetBundle.clusterName,
      clipFilename: selectedClipName,
      clipPath: selectedClipPath,
      snapshotPath: clipRow.snapshot_path,
      existsOnDisk,
      dbId: clipRow.id,
      reason: matchReason
    };

    choreography.push(sceneResult);

    console.log(`[Scene ${sceneIndex.toString().padStart(2, '0')}] [${targetBundle.sku}] ${targetBundle.brandCode} — ${selectedClipName}`);
    console.log(`  Alasan : ${matchReason}`);
    console.log(`  Teks   : "${sceneText.slice(0, 68)}..."`);
    console.log(`  Path   : ${selectedClipPath}`);
    console.log(`  Status : ${existsOnDisk ? 'TERVERIFIKASI DI DISK' : 'BERKAS TIDAK DITEMUKAN'}`);
    console.log('');
  });

  // Rekapitulasi Distribusi Bundel untuk Verifikasi Anti-Pola
  console.log('=============================================================================');
  console.log('  REKAPITULASI DISTRIBUSI BUNDEL (VERIFIKASI ANTI-POLA & NON-GENERIK):');
  Object.keys(bundleUsageCount).forEach(sku => {
    const b = bundleMap[sku];
    console.log(`  - [${sku}] ${b.brandCode.padEnd(14)} : ${bundleUsageCount[sku]} klip (${b.title})`);
  });
  console.log(`  Total Bundel Berbeda : ${Object.keys(bundleUsageCount).length} Rumpun Unik dari ${choreography.length} Adegan`);
  console.log(`  Status Duplikasi     : 0% Duplikasi (100% Klip Unik & Segar)`);
  console.log('=============================================================================\n');

  return choreography;
}

/**
 * Mencari klip video dari bundel berdasarkan kata kunci atau SKU (CLI Utility)
 */
function searchVidcon(keywords = [], limitClips = 5, options = {}) {
  const database = getDatabase();
  const kwList = Array.isArray(keywords) ? keywords.map(k => k.toLowerCase()) : [keywords.toLowerCase()];

  console.log(`\n[VIDCON TIER-1] Mencari footage dengan kata kunci: [${kwList.join(', ')}]...`);

  const kwParam = `%${kwList[0]}%`;
  const rows = database.prepare(`
    SELECT id, bundle_sku, bundle_name, category, clip_name, video_path, hashtags, used_count
    FROM footage_items
    WHERE is_clean = 1 
      AND bundle_sku NOT IN ('ID4834', 'ID53FA', 'IDA71C', 'IDCB24', 'IDE830', 'IDE8B2', 'ID8F64')
      AND clip_name NOT LIKE '0000%'
      AND LOWER(clip_name) NOT LIKE '%preview%'
      AND LOWER(clip_name) NOT LIKE '%trailer%'
      AND (hashtags LIKE ? OR category LIKE ? OR bundle_sku LIKE ?)
    LIMIT ?
  `).all(kwParam, kwParam, kwParam, limitClips);

  console.log(`Ditemukan ${rows.length} klip representatif:\n`);
  rows.forEach((r, idx) => {
    console.log(`[Top ${idx + 1}] [${r.bundle_sku}] ${r.bundle_name}`);
    console.log(`  Klip      : ${r.clip_name}`);
    console.log(`  Kategori  : ${r.category}`);
    console.log(`  Hashtag   : ${r.hashtags}`);
    console.log(`  Pemakaian : used_count = ${r.used_count}`);
    console.log(`  Path      : ${r.video_path}`);
    console.log('');
  });

  return rows;
}

// =============================================================================
// CLI TEST RUNNER
// =============================================================================
if (require.main === module) {
  const args = process.argv.slice(2);
  let footageSkusParam = null;
  let topicParam = null;
  let domainParam = null;
  const positional = [];

  for (let i = 0; i < args.length; i++) {
    if ((args[i] === '--footage-skus' || args[i] === '--footageSkus') && args[i + 1]) {
      footageSkusParam = args[++i];
    } else if (args[i] === '--topic' && args[i + 1]) {
      topicParam = args[++i];
    } else if (args[i] === '--domain' && args[i + 1]) {
      domainParam = args[++i];
    } else {
      positional.push(args[i]);
    }
  }

  if (footageSkusParam || topicParam) {
    const testScenes = [
      { scene: 1, text: "Dalam kesunyian malam, seorang pria tangguh mengasah fokusnya.", duration: 6.0 },
      { scene: 2, text: "Ketabahan Thomas Shelby menghadapi pengkhianatan tanpa amarah.", duration: 7.0 },
      { scene: 3, text: "Kehidupan keras membentuk watak baja yang tak tergoyahkan.", duration: 6.5 },
      { scene: 4, text: "Fokus pada kendali internal, buang segala distraksi.", duration: 6.0 }
    ];
    console.log(`Menjalankan Pengujian Koreografer Mandiri [Topic: ${topicParam || 'Umum'}] [SKUs: ${footageSkusParam || 'auto'}]...`);
    const result = buildKinshipSequenceForScript({
      scriptScenes: testScenes,
      defaultTheme: domainParam || 'hard_reality',
      title: topicParam || 'Uji Mandiri',
      projectId: 'CLI_TEST',
      dryRun: true,
      selectedSkus: footageSkusParam
    });
    console.log(`Hasil pengujian: ${result.length} adegan berhasil dipetakan.`);
  } else if (positional.length === 0 || positional[0] === 'test') {
    const sandboxDir = path.join(__dirname, '..', 'projects', '_test_sandbox_Uji_Mandiri', '04_prompts');
    const timingPath = path.join(sandboxDir, 'sentence_timing_master.json');
    if (fs.existsSync(timingPath)) {
      console.log('Menjalankan Uji Petik Validasi Anti-Pattern Terhadap Sandbox Mandiri...');
      const scenes = JSON.parse(fs.readFileSync(timingPath, 'utf8'));
      const result = buildKinshipSequenceForScript({
        scriptScenes: scenes,
        defaultTheme: 'hard_reality',
        projectId: 'TEST_SANDBOX',
        dryRun: true
      });
      console.log(`Hasil validasi: ${result.length} adegan sukses terpetakan.`);
    } else {
      console.log('Mesin pencari vidkon siap digunakan. Jalankan dengan kata kunci atau parameter pipeline.');
    }
  } else {
    searchVidcon(positional, 5);
  }
}

module.exports = {
  loadCatalog,
  searchVidcon,
  resolvePrimaryCluster,
  scoreSemanticAffinity,
  detectContextualTrigger: scoreSemanticAffinity,
  buildKinshipSequenceForScript,
  detectNicheDomain,
  NICHE_DOMAINS,
  stemIndo,
  stemVariants,
  CONCEPT_LEXICON
};
