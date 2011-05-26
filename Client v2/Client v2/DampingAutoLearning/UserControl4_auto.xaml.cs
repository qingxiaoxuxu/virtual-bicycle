using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using BuzzWin;
using AForge.Neuro;
using AForge.Neuro.Learning;

namespace Client_v2.DampingAutoLearning
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    
    
    public partial class UserControl1 : UserControl
    {
        ActivationNetwork an = new ActivationNetwork(new SigmoidFunction(), 4, 4, 3, 1);
        BackPropagationLearning bp;
        double predump = 0;
        int time = 0;
        BuzzWin.DeviceDataManager.Damp data = new DeviceDataManager.Damp();
        static int DataCount = 0;
        static bool LearningMode = true;
        static int TrainingCount = 20;
        public UserControl1()
        {
            InitializeComponent();
        }
        public void Init()
        {
            an.Randomize();
            bp = new BackPropagationLearning(an);
            bp.LearningRate = 0.5;
            InfoControl.device.GetSportStatus += new DeviceDataManager.F2(device_GetSportStatus);
            InfoControl.device.GetGameControl += new DeviceDataManager.F8(device_GetGameControl);
            data.value=0;
            InfoControl.device.SetDamp(data);
        }
         static int Shift = 0;
        void device_GetGameControl(DeviceDataManager.GameControl gameControl)
        {
            BuzzWin.DeviceDataManager.Damp d = new DeviceDataManager.Damp();
            
            if (gameControl.Btn2)
            {
                if (Shift < 255)
                {
                    Shift += 1;
                    d.value = Shift;
                    InfoControl.device.SetDamp(d);
                }
            }
            if (gameControl.Btn1)
            {
                if (Shift > 0)
                {
                    Shift -= 1;
                    d.value = Shift;
                    InfoControl.device.SetDamp(d);
                }
            }
        }
        int preLoad = 0;
        void device_GetSportStatus(DeviceDataManager.SportStatus sportStatus)
        {
            if (LearningMode)
            {

                double[] input = { (double)DataCount/TrainingCount,(double)sportStatus.Speed / 20, (double)sportStatus.HeartRate / byte.MaxValue, (double)preLoad / byte.MaxValue };

                double[] output = { (double)Shift / byte.MaxValue };


                Console.WriteLine(sportStatus.load);
                Console.WriteLine(Shift);
                preLoad = Shift;
                inputList[DataCount] = input;
                outputList[DataCount] = output;
                DataCount++;
                if (DataCount == TrainingCount)
                {
                    LearningMode = false;
                    DataCount = 0;
                    for (int i = 0; i < 5000; i++)
                    {
                        double error = bp.RunEpoch(inputList, outputList);
                        //listBox1.Invoke(new Action(() =>
                        //{
                        //    listBox1.Items.Add(error);
                        //}));
                        Console.WriteLine(error);
                    }
                    MessageBox.Show("Learning Over");
                }
            }
            else
            {
                double[] input = { (double)DataCount / TrainingCount, (double)sportStatus.Speed / byte.MaxValue, /*(double)sportStatus.HeartRate*/(double)60 / byte.MaxValue, (double)preLoad / byte.MaxValue };
                double output = an.Compute(input)[0];
                //listBox2.Invoke(new Action(() =>
                //        {
                //            listBox2.Items.Add(((int)(output * 255)).ToString());
                //        }));
                BuzzWin.DeviceDataManager.Damp d = new DeviceDataManager.Damp();
                d.value=(int)(output*255);
                InfoControl.device.SetDamp(d);
                DataCount++;
            }
        }
        double[][] inputList=new double[TrainingCount][];
        double[][] outputList=new double[TrainingCount][];
        private void button1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 100; i++)
            {
                double[] input = { (double)i / 255, (double)(5) / 255, (double)(60) / 255, (double)i/255 };
                double[] output = { (double)(i + 1)/255 };
                inputList[i] =input;
                outputList[i] = output;
            }

            for (int i = 0; i < 5000; i++)
            {
                double error = bp.RunEpoch(inputList, outputList);
                listBox1.Items.Add(error);
            }

            timer1.Start();
            
            //int count = 0;
            //InputP[] inputList = new InputP[100];
            //OutputP[] outputList=new OutputP[100];
            //for (int i = 0; i < 50; i++)
            //{
            //    inputList[i] = new InputP((double)i / 100, (double)5/255, (double)60/255, (double)i / 100, new OutputP((double)i+1 / 100));
            //    outputList[i] = new OutputP((double)i + 1 / 100);
            //}
            //for (int i = 50; i < 100; i++)
            //{
            //    inputList[i] = new InputP((double)i / 100, (double)5 / 255, (double)60 / 255, (double)i / 100, new OutputP((double)i + 1 / 100));
            //    outputList[i] = new OutputP((double)i + 1 / 100);
            //}
            
            //while (!sdbp.IsTrainOver(inputList, outputList) )
            //{
            //    for (int i = 0; i < 100; i++)
            //    {

            //        sdbp.Training(inputList[i]);
            //    }
            //    count++;
            //}
            //timer1.Start();
            

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            time++;
            //InputP input = new InputP((double)time / 100, (double)5 / 255, (double)60 / 255, (double)time / 100, null);
            //OutputP output;
            
            //output = sdbp.Execute(input);
            //preDump = (byte)(output.p1*100);
            double[] input = { (double)time / 255, (double)(5 ) / 255, (double)(60 ) / 255, predump };
            double output = an.Compute(input)[0];
            predump = output;
            listBox2.Items.Add(time.ToString()+"  "+(predump*255).ToString());
            if (time == 255) timer1.Stop();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //创建文件流
            FileStream fStream=new FileStream("para.txt",FileMode.Create);

            //使用二进制序列化器
            BinaryFormatter binFormat = new BinaryFormatter();
            List<double> list = new List<double>();
            for (int i = 0; i < an.LayersCount;i++)
            {
                for (int j = 0; j < an[i].NeuronsCount; j++)
                {
                    for (int k = 0; k < an[i][j].InputsCount; k++)
                        list.Add(an[i][j][k]);
                }
            }
            binFormat.Serialize(fStream, list);
            fStream.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            LearningMode = false;   
            //创建文件流
            FileStream fStream = new FileStream("para.txt", FileMode.Open);

            //使用二进制序列化器
            BinaryFormatter binFormat = new BinaryFormatter();
            //将list序列化到文件中
            List<double> list = new List<double>();
            list=(List<double>)binFormat.Deserialize(fStream);
            int p = 0;
            for (int i = 0; i < an.LayersCount; i++)
            {
                for (int j = 0; j < an[i].NeuronsCount; j++)
                {
                    for (int k = 0; k < an[i][j].InputsCount; k++)
                    {
                        an[i][j][k] = list[p++];
                    }
                }
            }
            fStream.Close();
        }
    }
}
