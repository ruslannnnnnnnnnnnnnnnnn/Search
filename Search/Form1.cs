using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace Search
{
    public partial class Form1 : Form
    {
        Search search;
        LinkedList<Item> items;
        KeyValuePair<string, long>[] Res;

        int begin, end;
        int range;

        public Form1()
        {
            InitializeComponent();

            range = 10;

            items = new LinkedList<Item>();

            search = new Search(@"D:\текст\invert\dictonary.txt", @"D:\текст\invert\file.txt");
        }

        private void Search_Click(object sender, EventArgs e)
        {
            if (textBoxSearch.Text == null || textBoxSearch.Text == "")
            {
                MessageBox.Show("запрос пуст");
                return;
            }
            else
            {
                try
                {
                    var res = search.SearchBool(textBoxSearch.Text);

                    if (search.Error || !SortBox.Checked)
                        Res = res.Array;
                    else
                        Res = res.Array.OrderBy((elem) => elem.Value).ToArray();
                }
                catch
                {
                    MessageBox.Show("Ошибка ввода");
                    return;
                }
            }

            LengthText.Text = Res.Length.ToString();

            begin = 0;
            end = range;

            Items();
        }

        private void Prev_Click(object sender, EventArgs e)
        {
            if (begin == 0) return;

            begin -= range;
            end -= range;

            Items();
        }

        private void Next_Click(object sender, EventArgs e)
        {
            if (begin > Res.Length) return;

            begin += range;
            end += range;

            Items();
        }

        public void Items()
        {
            foreach (var item in items) item.Remove();

            items = new LinkedList<Item>();

            for (int i = begin, j = 0; i < Res.Length && i < end; i++, j++)
            {
                items.AddLast(new Item(i + 1, j, string.Format(@"D:\текст\text{0}.txt", Res[i].Key), Controls));
            }
        }
    }

    public class Item
    {
        public GroupBox GroupBox;
        public Label Label;
        public Button Button;
        public Control.ControlCollection Controls;
        public Button ButtonUrl;

        public Item(int groupBox, int i, string file, Control.ControlCollection controls)
        {        
            GroupBox = new GroupBox();
            GroupBox.Text = groupBox.ToString();

            GroupBox.Location = new Point(100, 100 + 50 * i);
            GroupBox.Size = new Size(590, 50);

            Label = new Label();
            Label.Text = file;

            Label.Location = new Point(100, 20);
            Label.AutoSize = true;

            Button = new Button();
            Button.Click += (sender, e) =>
            {
                Process.Start(file);
            };

            Button.Text = "просмотр";

            Button.Location = new Point(410, 20);

            ButtonUrl = new Button();
            ButtonUrl.Click += (sender, e) =>
            {
                using (var str = new StreamReader(file))
                {
                    string url = str.ReadLine();
                    Process.Start("IExplore.exe", url);
                }
            };

            ButtonUrl.Text = "страница";

            ButtonUrl.Location = new Point(500, 20);

            controls.Add(GroupBox);
            GroupBox.Controls.Add(Label);
            GroupBox.Controls.Add(Button);
            GroupBox.Controls.Add(ButtonUrl);

            Controls = controls;
        }

        public void Remove()
        {
            Controls.Remove(GroupBox);
        }
    }
}
