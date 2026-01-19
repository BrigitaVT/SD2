using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SD2
{
    public partial class SD2 : Form
    {
        public SD2()
        {
            InitializeComponent();

            cmbMode.Items.Clear();
            cmbMode.Items.Add("Vidurkis");
            cmbMode.Items.Add("Mediana");
            cmbMode.Items.Add("Abu");
            cmbMode.SelectedIndex = 0;

            txtOutput.ReadOnly = true;
        }

        private void atidarytiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                txtInput.Text = File.ReadAllText(openFileDialog1.FileName, Encoding.UTF8);
            }
        }

        private void išsaugotiĮvestįToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog1.FileName, txtInput.Text, Encoding.UTF8);
            }
        }

        private void išsaugotiRezultatusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog1.FileName, txtOutput.Text, Encoding.UTF8);
            }
        }

        private void uždarytiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void apieProgramąToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Studentų pažymių skaičiavimo programa (SD2)\nKūrėja: Jūsų vardas pavardė\nOrganizacija: VVK",
                "Apie programą",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            try
            {
                var students = ParseStudents(txtInput.Text);

                if (students.Count == 0)
                {
                    txtOutput.Text = "Nėra duomenų skaičiavimui.";
                    return;
                }

                foreach (var s in students)
                {
                    s.FinalAvg = 0.4 * s.Homeworks.Average() + 0.6 * s.Exam;
                    s.FinalMed = 0.4 * Median(s.Homeworks) + 0.6 * s.Exam;
                }

                students = students
                    .OrderBy(x => x.Surname, StringComparer.CurrentCulture)
                    .ThenBy(x => x.Name, StringComparer.CurrentCulture)
                    .ToList();

                txtOutput.Text = BuildOutput(students, cmbMode.SelectedIndex);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Klaida", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private class Student
        {
            public string Name { get; set; }
            public string Surname { get; set; }
            public List<int> Homeworks { get; set; }
            public int Exam { get; set; }
            public double FinalAvg { get; set; }
            public double FinalMed { get; set; }
        }

        private static List<Student> ParseStudents(string text)
        {
            var lines = text
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .Where(l => l.Length > 0)
                .ToList();

            if (lines.Count == 0) return new List<Student>();

            bool header = lines[0].Contains("ND") || lines[0].Contains("Egz") || lines[0].Contains("Egz.");
            if (header) lines.RemoveAt(0);

            var res = new List<Student>();

            foreach (var line in lines)
            {
                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 4) continue;

                string name = parts[0];
                string surname = parts[1];

                var nums = parts.Skip(2)
                    .Select(p =>
                    {
                        if (!int.TryParse(p, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v))
                            throw new Exception("Neteisingas skaičius eilutėje:\n" + line);
                        return v;
                    })
                    .ToList();

                if (nums.Count < 2)
                    throw new Exception("Trūksta ND/egzamino reikšmių eilutėje:\n" + line);

                int egz = nums.Last();
                var nd = nums.Take(nums.Count - 1).ToList();

                if (nd.Count == 0)
                    throw new Exception("Trūksta namų darbų reikšmių eilutėje:\n" + line);

                res.Add(new Student
                {
                    Name = name,
                    Surname = surname,
                    Homeworks = nd,
                    Exam = egz
                });
            }

            return res;
        }

        private static double Median(List<int> v)
        {
            var a = v.OrderBy(x => x).ToList();
            int n = a.Count;
            if (n % 2 == 1) return a[n / 2];
            return (a[n / 2 - 1] + a[n / 2]) / 2.0;
        }

        private static string BuildOutput(List<Student> students, int mode)
        {
            var sb = new StringBuilder();

            if (mode == 0) sb.AppendLine("Pavarde\tVardas\tGalutinis(Vid.)");
            else if (mode == 1) sb.AppendLine("Pavarde\tVardas\tGalutinis(Med.)");
            else sb.AppendLine("Pavarde\tVardas\tGalutinis(Vid.)\tGalutinis(Med.)");

            sb.AppendLine(new string('-', 60));

            foreach (var s in students)
            {
                if (mode == 0)
                    sb.AppendLine($"{s.Surname}\t{s.Name}\t{s.FinalAvg:F2}");
                else if (mode == 1)
                    sb.AppendLine($"{s.Surname}\t{s.Name}\t{s.FinalMed:F2}");
                else
                    sb.AppendLine($"{s.Surname}\t{s.Name}\t{s.FinalAvg:F2}\t{s.FinalMed:F2}");
            }

            return sb.ToString();
        }
    }
}
