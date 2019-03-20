using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Security;

namespace Laba1_TheoryOfInfirmation_V_8
{
    public partial class Form1 : Form
    {
        
        private OpenFileDialog openFileDialog1;
        public string FileName , TextMessage;
        

        public Form1()
        {
            InitializeComponent();
        }

        private void LoadMessage_Click(object sender, EventArgs e) // Загрузка повідомлення та проведення обчислень
        {
            openFileDialog1 = new OpenFileDialog(); 

            if(openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FileName = openFileDialog1.FileName;
                MessageBox.Show(FileName);

                StreamReader strReader = new StreamReader(FileName , Encoding.GetEncoding(1251)); // Відкриття потоку
                TextMessage =  strReader.ReadToEnd(); // Зчитування текстового файлу
               List<NodeSymbol> symbols = FillListBox(TextMessage); // контейнер для збереження повідомлень та іншої інформації
               double Entropia = CalculateEntropia(symbols); // Знаходження ентропії повідомлень
                InfoBox.Text = (Entropia*symbols.Count).ToString(); // Виведення ентропії

            
                SortSymbols(ref symbols); // Сортування повідомлень за спаданням ймовірності 
                Fano(0, symbols.Count -1, ref symbols); // алгоритм кодування за Шеноном Фано
                for (int i = 0; i < symbols.Count; i++)
                {
                    symbols[i].SetFano();
                }

                double EffectiveLength = calculateEffectiveLength(symbols); // визначення середньої кількості символів на повідомлення
                EffectiveLengthBox.Text = EffectiveLength.ToString(); // виведення значення L.ефек.сер

                textBox1.Text = Entropia.ToString();


                strReader.Close(); // Закриття потоку
            }
        }

        private void SortSymbols(ref List<NodeSymbol> symbols) // Сортування символів в порядку спадання ймовірності
        {
            string tempSymbol = "";
            int tempCounter = 0;
            double tempProbability = 0.0;
            for(int i = 0; i < symbols.Count; i++)
            {
                for(int j = 0; j < symbols.Count-1; j++)
                {
                    if(symbols[j].Probability < symbols[j + 1].Probability)
                    {
                        tempSymbol = symbols[j].Symbol;
                        tempCounter = symbols[j].CounterShow;
                        tempProbability = symbols[j].Probability;

                        symbols[j].Probability = symbols[j +1].Probability;
                        symbols[j].Symbol = symbols[j+1].Symbol;
                        symbols[j].CounterShow = symbols[j+1 ].CounterShow;

                        symbols[j + 1].Probability = tempProbability;
                        symbols[j + 1].CounterShow = tempCounter;
                        symbols[j + 1].Symbol = tempSymbol;
                    }
                }
            }
        }

        public double calculateEffectiveLength(List<NodeSymbol> symbols) // Знаходження значення L.сер.еф
        {
            double result = 0;
            for(int i = 0; i < symbols.Count; i++)
            {
                result += symbols[i].Probability * symbols[i].Fano.Count();
            }
            return result;
        }

        private List<NodeSymbol> FillListBox(string TextMessage) // Заповнення таблиці значень та підрахунок ймовірності зустрічання повідомлень
        {
            int SymbolLength = TextMessage.Length;
            List<NodeSymbol> symbols = new List<NodeSymbol>();
            List<char> UniqueSymbol = new List<char>();
            for(int i = 0; i < TextMessage.Length; i++)
            {
                
                char temp = TextMessage[i];
                if (temp == '\r' || temp == '\n') { SymbolLength--; continue; }
                if (UniqueSymbol.Contains(temp))
                {
                    SeekSymbol(ref symbols, temp);
                }
                else { symbols.Add(new NodeSymbol(temp, 0, 0)); UniqueSymbol.Add(temp); }
            }
            CalculateProbability(ref symbols, SymbolLength);

            var bindinglist = new BindingList<NodeSymbol>(symbols);
            var source = new BindingSource(bindinglist, null);
            dataGridView1.DataSource = source;
            return symbols;
        }
        private void CalculateProbability(ref List<NodeSymbol> symbols , int symbolLength) // Підрахунок ймовірностей повідомлень
        {
            for(int i = 0; i < symbols.Count; i++)
            {
                symbols[i].Probability = Math.Round((double)symbols[i].Counter / symbolLength , 3);
            }
        }
      
        public double CalculateEntropia(List<NodeSymbol> symbols) // Підрахунок ентропії
        {
            double result = 0;
            for(int i = 0; i < symbols.Count; i++)
            {
                result += symbols[i].Probability * Math.Log(symbols[i].Probability, 2);
            }
            result = Math.Abs(result);
            result = Math.Round(result, 3);
            var ForEntropiaBox = result.ToString();
            EntropiaBox.Text = ForEntropiaBox;
            return result;
        }

        private void SeekSymbol( ref List<NodeSymbol> symbols , char temp)
        {
            for(int i = 0; i < symbols.Count; i++)
            {
                if(symbols[i].Symbol == temp.ToString())
                {
                    symbols[i].Counter++;
                    symbols[i].CounterShow++;
                    break;
                }
            }
        }


        double count1 = 0, count2 = 0; int m; int g = 0;

        public int DividePoslidovnist(int L , int R , ref List<NodeSymbol> symbols) // Розділення послідовності символів на рівноймовірні частини
        {
            count1 = 0;
            for(int i = L; i <= R-1; i++)
            {
                count1 += symbols[i].Probability;
            }

            count2 = symbols[R].Probability;
            m = R;
            while(count1 >= count2)
            {
                m = m - 1;
                count1 -= symbols[m].Probability;
                count2 += symbols[m].Probability;
            }
            return m;

        }

        public void Fano(int L , int R , ref List<NodeSymbol> symbols) // Алгоритм Шеннона Фано
        {
            int n;

            if(L < R)
            {
                n = DividePoslidovnist(L, R, ref symbols);
                for(int i = L; i<= R; i++)
                {
                    if(i <= n)
                    {
                        symbols[i].Res  += Convert.ToByte(1);
                    }
                    else
                    {
                        symbols[i].Res += Convert.ToByte(0);
                    }
                }
                Fano(L, n , ref symbols);
                Fano(n + 1, R, ref symbols);
            }
        }

       
        }
    }

    public class NodeSymbol // Клас для збереження символу, ймовірності, коду
    {
        public string Symbol { get; set; }
        public int Counter = 0;
        public double Probability { get; set; }
        public int CounterShow { get; set; }
    public string Res = "";
    public string Fano { get; set; }
        public NodeSymbol(char symbol , int counter , int probability)
        {
            Symbol = symbol.ToString();
            Counter = 1;
            CounterShow = Counter;
            Probability = probability;
        }
    public void SetFano()
    {
        Fano = Res;
    }
    }

   


