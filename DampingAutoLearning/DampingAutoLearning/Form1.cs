using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AForge.Neuro;
using AForge.Neuro.Learning;

namespace DampingAutoLearning
{
    public partial class Form1 : Form
    {
        //public SDBP sdbp = new SDBP();
        ActivationNetwork an = new ActivationNetwork(new SigmoidFunction(), 4, 4, 3, 1);
        BackPropagationLearning bp;
        double predump = 0;
        int time = 0;
        public Form1()
        {
            InitializeComponent();
            timer1.Interval = 100;
            an.Randomize();
            bp = new BackPropagationLearning(an);
            bp.LearningRate = 0.5;
        }
        double[][] inputList=new double[100][];
        double[][] outputList=new double[100][];
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
    }
}
