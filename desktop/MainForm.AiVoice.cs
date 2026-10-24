using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using SmartVidkonAutomation.Helpers;

namespace SmartVidkonAutomation
{
    public partial class MainForm
    {
        private static readonly string[] GeminiModels = new string[]
        {
            "gemini-2.5-flash",
            "gemini-2.0-flash",
            "gemini-1.5-flash"
        };

        private async Task GenerateScriptFromGeminiAsync()
        {
            string topic = _txtStudioTopic.Text.Trim();
            if (string.IsNullOrEmpty(topic))
            {
                MessageBox.Show("Silakan masukkan topik konten terlebih dahulu sebelum membuat naskah.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtStudioTopic.Focus();
                return;
            }

            int durIdx = (_cbStudioDuration != null) ? _cbStudioDuration.SelectedIndex : 2;
            int targetScenes = 24;
            int targetWords = 380;
            if      (durIdx == 0) { targetScenes = 12; targetWords = 160; }
            else if (durIdx == 1) { targetScenes = 18; targetWords = 260; }
            else if (durIdx == 2) { targetScenes = 24; targetWords = 380; }
            else if (durIdx == 3) { targetScenes = 40; targetWords = 600; }
            else if (durIdx == 4) { targetScenes = 80; targetWords = 1300; }

            string theme = (_cbStudioTheme != null && _cbStudioTheme.SelectedItem != null) ? _cbStudioTheme.SelectedItem.ToString() : "Stoikisme & Filsafat";
            string audience = (_cbStudioAudience != null && _cbStudioAudience.SelectedItem != null) ? _cbStudioAudience.SelectedItem.ToString() : "Universal";

            _btnStudioGenAi.Enabled = false;
            _btnStudioGenAi.Text = "Meracik Naskah...";

            AppendStudioLog("═══════════════════════════════════════════════════════════════");
            AppendStudioLog(string.Format("  [GEMINI AI ENGINE] Meracik Naskah Video Edukasi"));
            AppendStudioLog(string.Format("  • Topik     : {0}", topic));
            AppendStudioLog(string.Format("  • Tema      : {0}", theme));
            AppendStudioLog(string.Format("  • Target    : {0} Adegan (~{1} Kata)", targetScenes, targetWords));
            AppendStudioLog("═══════════════════════════════════════════════════════════════");

            List<string> geminiKeys = SmartApiVault.GetStudioGeminiKeys();
            string generatedScript = null;

            await Task.Run(async delegate
            {
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12 | (System.Net.SecurityProtocolType)3072;

                string prompt = string.Format(
                    "Anda adalah penulis naskah video vertikal edukasi 9:16 kelas dunia berfokus pada kedalaman emosi, refleksi psikologis, dan kebijaksanaan hidup stoik.\n\n" +
                    "Topik: \"{0}\"\n" +
                    "Tema Klaster: {1}\n" +
                    "Target Audiens: {2}\n" +
                    "Target Jumlah Baris: Tepat {3} baris (1 baris per adegan video).\n" +
                    "Panjang total: Sekitar {4} kata.\n\n" +
                    "ATURAN MUTLAK:\n" +
                    "1. Setiap baris harus berupa SATU kalimat utuh tanpa nomor, tanpa tanda bintang/bold, tanpa instruksi kamera.\n" +
                    "2. Gunakan Bahasa Indonesia yang sangat indah, tenang, menyentuh batin, dan berwibawa.\n" +
                    "3. Buat pembuka (hook) yang langsung mencengkeram perhatian di baris pertama.\n" +
                    "4. Jangan gunakan bullet points atau angka di awal baris.\n" +
                    "Hanya keluarkan teks naskah baris per baris tanpa kata pengantar.",
                    topic, theme, audience, targetScenes, targetWords);

                foreach (string apiKey in geminiKeys)
                {
                    if (string.IsNullOrEmpty(apiKey)) continue;

                    foreach (string model in GeminiModels)
                    {
                        try
                        {
                            using (HttpClient client = new HttpClient())
                            {
                                client.Timeout = TimeSpan.FromSeconds(20);
                                string url = string.Format("https://generativelanguage.googleapis.com/v1beta/models/{0}:generateContent?key={1}", model, apiKey);

                                Dictionary<string, object> reqBody = new Dictionary<string, object>();
                                List<object> contents = new List<object>();
                                Dictionary<string, object> contentPart = new Dictionary<string, object>();
                                List<object> parts = new List<object>();
                                Dictionary<string, object> textPart = new Dictionary<string, object>();
                                textPart.Add("text", prompt);
                                parts.Add(textPart);
                                contentPart.Add("parts", parts);
                                contents.Add(contentPart);
                                reqBody.Add("contents", contents);

                                JavaScriptSerializer js = new JavaScriptSerializer();
                                string jsonReq = js.Serialize(reqBody);

                                StringContent httpContent = new StringContent(jsonReq, Encoding.UTF8, "application/json");
                                HttpResponseMessage resp = await client.PostAsync(url, httpContent);

                                if (resp.IsSuccessStatusCode)
                                {
                                    string respBody = await resp.Content.ReadAsStringAsync();
                                    Dictionary<string, object> dict = js.Deserialize<Dictionary<string, object>>(respBody);

                                    if (dict != null && dict.ContainsKey("candidates"))
                                    {
                                        System.Collections.ArrayList candArr = dict["candidates"] as System.Collections.ArrayList;
                                        if (candArr != null && candArr.Count > 0)
                                        {
                                            Dictionary<string, object> firstCand = candArr[0] as Dictionary<string, object>;
                                            if (firstCand != null && firstCand.ContainsKey("content"))
                                            {
                                                Dictionary<string, object> candContent = firstCand["content"] as Dictionary<string, object>;
                                                if (candContent != null && candContent.ContainsKey("parts"))
                                                {
                                                    System.Collections.ArrayList pArr = candContent["parts"] as System.Collections.ArrayList;
                                                    if (pArr != null && pArr.Count > 0)
                                                    {
                                                        Dictionary<string, object> firstP = pArr[0] as Dictionary<string, object>;
                                                        if (firstP != null && firstP.ContainsKey("text"))
                                                        {
                                                            generatedScript = firstP["text"].ToString();
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch { }

                        if (!string.IsNullOrEmpty(generatedScript)) break;
                    }

                    if (!string.IsNullOrEmpty(generatedScript)) break;
                }
            });

            _btnStudioGenAi.Enabled = true;
            _btnStudioGenAi.Text = "Buat Naskah (AI)";

            if (!string.IsNullOrEmpty(generatedScript))
            {
                _txtStudioScript.Text = generatedScript.Trim();
                SplitAndFormatScriptScenes();
                AppendStudioLog("  Naskah edukasi berhasil dibuat dan diformat otomatis per adegan.");
            }
            else
            {
                AppendStudioLog("[AI] Server Gemini tidak dapat dijangkau (Offline/Disconnect). Memuat naskah cerdas lokal berkualitas tinggi...");
                LoadSampleScript();
            }
        }

        private async Task CheckElevenLabsQuotaAsync()
        {
            _btnStudioCheckAiQuota.Enabled = false;
            _btnStudioCheckAiQuota.Text = "Mengecek Kuota...";

            try
            {
                List<string> keys = SmartApiVault.GetStudioElevenLabsKeys();
                AppendStudioLog("═══════════════════════════════════════════════════════════════");
                AppendStudioLog("  [STATUS KUOTA ELEVENLABS & MODEL]");
                foreach (string k in keys)
                {
                    string status = await SmartApiVault.TestElevenLabsKeyAsync(k);
                    string mask = (k.Length > 8) ? k.Substring(0, 4) + "••••" + k.Substring(k.Length - 4) : k;
                    AppendStudioLog(string.Format("  • Kunci {0}: {1}", mask, status));
                }
                AppendStudioLog("═══════════════════════════════════════════════════════════════");
            }
            finally
            {
                _btnStudioCheckAiQuota.Enabled = true;
                _btnStudioCheckAiQuota.Text = "Cek Model dan Kuota";
            }
        }

        private void PolishScriptProsody()
        {
            string raw = _txtStudioScript.Text;
            if (string.IsNullOrEmpty(raw))
            {
                MessageBox.Show("Kotak naskah masih kosong.", "Poles Prosody", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string[] lines = raw.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> polished = new List<string>();

            foreach (string line in lines)
            {
                string t = line.Trim();
                if (string.IsNullOrEmpty(t)) continue;

                // Hilangkan nomor bullet (1., 2., -, •)
                t = System.Text.RegularExpressions.Regex.Replace(t, @"^[\d\.\-\•\*\#\s]+", "").Trim();
                if (string.IsNullOrEmpty(t)) continue;

                // Pastikan huruf kapital di awal
                if (t.Length > 1)
                {
                    t = char.ToUpper(t[0]) + t.Substring(1);
                }

                // Tambahkan tanda titik jika belum ada tanda baca di akhir
                if (!t.EndsWith(".") && !t.EndsWith("!") && !t.EndsWith("?"))
                {
                    t += ".";
                }

                polished.Add(t);
            }

            _txtStudioScript.Text = string.Join(Environment.NewLine, polished.ToArray());
            UpdateSentenceCounter();
            AppendStudioLog(string.Format("[POLISH] Prosody naskah diperbarui ({0} adegan terstruktur).", polished.Count));
        }
    }
}
