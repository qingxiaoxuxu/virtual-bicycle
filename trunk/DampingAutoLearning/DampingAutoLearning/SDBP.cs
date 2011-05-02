using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace DampingAutoLearning
{
    public class SDBP
    {
        private double[,] w1 = new double[8, 4];
        private double[,] w2 = new double[1, 8];
        private double[,] b1 = new double[8, 1];
        private double[,] b2 = new double[1, 1];
        private Random ran = new Random();

        public SDBP()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    w1[i, j] = ran.NextDouble() ;
                }
            }
            for (int i = 0; i < 1; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    w2[i, j] = ran.NextDouble() ;
                }
            }
            for (int i = 0; i < 8; i++)
            {
                b1[i, 0] = ran.NextDouble() ;
                
            }
            b2[0, 0] = ran.NextDouble();
        }

        private double fun1(double x)
        {
            return (double)1/(Math.Exp(-x)+1);
        }
        private double fun2(double x)
        {
            return x;
        }
        private double dfun1(double dx)
        {
            return (dx * (1 - dx));
        }
        private double dfun2(double dx)
        {
            return 1;
        }

        public void Training(InputP input)
        {
            double[,] a1 = new double[ 8,1];
            double[,] a2 = new double[ 1,1];
            a1 = Matrix.Mult(w1, input.vector);
            a1 = Matrix.Plus(b1, a1);
            for (int i = 0; i < a1.GetLength(0); i++)
            {
                a1[i, 0] = fun1(a1[i, 0]);
            }
            a2 = Matrix.Mult(w2, a1);
            a2 = Matrix.Plus(b2, a2);
            for (int i = 0; i < a2.GetLength(0); i++)
            {
                a2[i, 0] = fun2(a2[i, 0]);
            }
            double[,] e = new double[1, 1];
            e = Matrix.Minus(input.o.vector, a2);
            double[,] s2 = new double[1, 1];
            double[,] s1 = new double[8, 8];
            double[,] ss1 = new double[8, 1];
            double[,] ss2 = new double[1, 1];
            double[,] con=new double[1,1];
            con[0,0]=-2;
            for (int i = 0; i < 1; i++)
            {
                s2[i, i] = dfun2(a2[i, 0]);
            }
            ss2 = Matrix.Mult(s2, e);
            ss2 =Matrix.Mult(ss2,con);
            for (int i = 0; i < 8; i++)
            {
                s1[i, i] = dfun1(a1[i, 0]);
            }
            ss1 = Matrix.Mult(Matrix.Inv(w2),ss2);
            ss1 = Matrix.Mult(s1,ss1);
            con[0, 0] = 0.5;
            w1 = Matrix.Minus(w1, Matrix.Mult(Matrix.Mult( ss1,con), Matrix.Inv(input.vector)));
            w2 = Matrix.Minus(w2, Matrix.Mult(Matrix.Mult( ss2,con), Matrix.Inv(a1)));
            b1 = Matrix.Minus(b1, Matrix.Mult( ss1,con));
            b2 = Matrix.Minus(b2, Matrix.Mult( ss2,con));
        }

        public bool IsTrainOver(InputP[] input, OutputP[] output)
        {
            double error = 0;
            OutputP temp;
            for (int i = 0; i < input.Length; i++)
            {
                temp=this.Execute(input[i]);
                error += Math.Abs(temp.p1 - output[i].p1);
            }
            if (error < 0.5)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public OutputP Execute(InputP input)
        {
            double[,] a1 = new double[8, 1];
            double[,] a2 = new double[1, 1];
            a1 = Matrix.Mult(w1, input.vector);
            a1 = Matrix.Plus(b1, a1);
            for (int i = 0; i < a1.GetLength(0); i++)
            {
                a1[i, 0] = fun1(a1[i, 0]);
            }
            a2 = Matrix.Mult(w2, a1);
            a2 = Matrix.Plus(b2, a2);
            for (int i = 0; i < a2.GetLength(0); i++)
            {
                a2[i, 0] = fun2(a2[i, 0]);
            }
            return new OutputP(a2[0, 0]);
        }
        public void Save(String path)
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        sw.WriteLine(w1[i, j]);
                    }
                }
                for (int i = 0; i < 1; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        sw.WriteLine(w2[i, j]);
                    }
                }
                for (int i = 0; i < 8; i++)
                {
                    sw.WriteLine(b1[i, 0]);
                }
                for (int i = 0; i < 1; i++)
                {
                    sw.WriteLine(b2[i, 0]);
                }
            }
        }
        public void Load(String path)
        {
            using (StreamReader sr = new StreamReader(path))
            {
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        w1[i,j]=Convert.ToDouble (sr.ReadLine());
                    }
                }
                for (int i = 0; i < 1; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        w2[i, j] = Convert.ToDouble(sr.ReadLine());
                    }
                }
                for (int i = 0; i < 8; i++)
                {
                    b1[i, 0] = Convert.ToDouble(sr.ReadLine());
                }
                for (int i = 0; i < 1; i++)
                {
                    b2[i, 0] = Convert.ToDouble(sr.ReadLine());
                }
            }
        }
    }
    public class InputP
    {
        public double p1;
        public double p2;
        public double p3;
        public double p4;
        public OutputP o;
        public double[,] vector = new double[4,1];
        public InputP()
        {
        }
        public InputP(double p1, double p2, double p3, double p4,OutputP o)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
            this.p4 = p4;
            this.o = o;
            vector[0, 0] = p1;
            vector[1, 0] = p2;
            vector[2, 0] = p3;
            vector[3, 0] = p4;
        }
        public void GenerateVector()
        {
            vector[0, 0] = p1;
            vector[1, 0] = p2;
            vector[2, 0] = p3;
            vector[3, 0] = p4;
        }
    }
    public class OutputP
    {
        public double p1;
        public double[,] vector = new double[1, 1];
        public OutputP(double p1)
        {
            this.p1 = p1;
            
            vector[0, 0] = p1;
            
        }
    }
    public class Matrix
    {
        public static double[,] Plus(double[,] x, double[,] y)
        {
            double[,] z = new double[x.GetLength(0), x.GetLength(1)]; ;
            for (int i = 0; i < x.GetLength(0); i++)
            {
                for (int j = 0; j < x.GetLength(1); j++)
                {
                    z[i, j] = x[i, j] + y[i, j];
                }
            }
            return z;
        }
        public static double[,] Minus(double[,] x, double[,] y)
        {
            double[,] z = new double[x.GetLength(0), x.GetLength(1)]; ;
            for (int i = 0; i < x.GetLength(0); i++)
            {
                for (int j = 0; j < x.GetLength(1); j++)
                {
                    z[i, j] = x[i, j] - y[i, j];
                }
            }
            return z;
        }
        public static double[,] Mult(double[,] x, double[,] y)
        {
            double[,] z = new double[x.GetLength(0), y.GetLength(1)];
            for (int i = 0; i < x.GetLength(0); i++)
            {
                for (int j = 0; j < y.GetLength(1); j++)
                {
                    for (int k = 0; k < y.GetLength(0); k++)
                    {
                        z[i, j] += x[i, k] * y[k, j];
                    }
                }
            }
            return z;
        }
        public static double[,] Inv(double[,] x)
        {
            double[,] z = new double[x.GetLength(1), x.GetLength(0)];
            for (int i = 0; i < x.GetLength(0); i++)
            {
                for (int j = 0; j < x.GetLength(1); j++)
                {
                    z[j, i] = x[i, j];
                }
            }
            return z;
        }
   
    }
}
