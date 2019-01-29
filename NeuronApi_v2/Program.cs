using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace NeuronApi_v2
{
    class Program {
        public static readonly Random random = new Random();
        static void Main(string[] args)
        {
            //XORProblem.Start();
            AutoEncoding encoding = new AutoEncoding();
        }
    }
    class XORProblem
    {
        public static void Start()
        {
            int InputsCount = 2;
            int[] HiddensCount = { 4, 4 };
            int OutputsCount = 1;
            float Epsilon = 0.7f;
            float Alpha = 0.3f;

            Brain brain = new Brain(InputsCount, OutputsCount, HiddensCount, Epsilon, Alpha);

            Console.Write("\nNeural network is seems to be inited, proceed? (y/n): ");
            var ans = Console.ReadLine();
            switch (ans.ToLower())
            {
                case "y":
                    break;
                case "n":
                    return;
                    break;
            }
            Console.Clear();
            Console.Write("Please, enter count of iterations to train: ");
            var iterations = int.Parse(Console.ReadLine());
            Console.Clear();

            for (int i = 0; i < iterations; i++)
            {
                Console.WriteLine("{0} out of {1} ({2}% done)\t\t", i, iterations, 100 * (float)i / iterations);

                //initing inputs and perfects values
                int first = Program.random.Next(0, 2);
                int second = Program.random.Next(0, 2);

                int perfect = first ^ second;

                float[] inputs = { first, second };
                float[] perfects = { perfect };

                float error = brain.DoIteration(perfects, inputs);
                brain.DrawOutputs();
                Console.WriteLine("Inputs: {0}, {1}", first, second);
                Console.WriteLine("XOR result: {0}", perfect);
                Console.WriteLine();
                Console.WriteLine("Outputs is: ");
                Console.WriteLine("\t" + string.Join("\n\t", brain.GetOutput()) + "\t\t");
                Console.WriteLine("\nError is: {0}%\t\t\n\n", error * 100);
                Console.SetCursorPosition(0, 0);
                //Thread.Sleep(10);
                brain.ClearInputs();
            }

            Console.Clear();
            Console.WriteLine("Go and try it out!");
            Console.ReadLine();
            while (true)
            {
                Console.SetCursorPosition(0, 1);
                Console.Write("Enter first value: ");
                int first = int.Parse(Console.ReadLine());
                Console.Write("Enter second value: ");
                int second = int.Parse(Console.ReadLine());

                int perfect = first ^ second;

                float[] inputs = { first, second };
                float[] perfects = { perfect };

                float error = brain.DoProbe(perfects, inputs);
                brain.DrawOutputs();
                Console.WriteLine("Inputs: {0}, {1}", first, second);
                Console.WriteLine("XOR result: {0}", perfect);
                Console.WriteLine();
                Console.WriteLine("Outputs is: ");
                Console.WriteLine("\t" + string.Join("\n\t", brain.GetOutput()) + "\t\t");
                Console.WriteLine("\nError is: {0}%\t\t\n\n", error * 100);
                //Thread.Sleep(10);
                brain.ClearInputs();
            }
        }
    }
    class AutoEncoding
    {
        static readonly string setFolder = "set/";
        static readonly int setSize = 385;
        public AutoEncoding()
        {
            float Epsilon = 1.0f;
            float Alpha = 0.3f;

            int InputCount = 32 * 32;
            List<int> HiddensCount = new List<int>();

            for (int i = -31; i < 32; i+= 3)
            {
                int c = 0;
                if (i == 0)
                {
                    c = (int)MathF.Sqrt(MathF.Pow(i, 2) + 1);
                }
                else
                {
                    c = (int)MathF.Floor(MathF.Sqrt(MathF.Pow(i, 2) + 1) + 1);
                }
                HiddensCount.Add(32 * c);
            }
            int OutputCount = 32 * 32;

            Brain brain = new Brain(InputCount, OutputCount, HiddensCount.ToArray(), Epsilon, Alpha, false);

            int maxIterations = 10000;
            int Out = 0;
            for (int iter = 0; iter < maxIterations; iter++)
            {
                Console.Write("\rIteration: {0}", iter);
                float[] inputsR = new float[InputCount];
                float[] inputsG = new float[InputCount];
                float[] inputsB = new float[InputCount];
                using (Bitmap bitmap = new Bitmap(setFolder + Program.random.Next(0, setSize + 1)))
                { 
                    for (int i = 0; i < InputCount; i++)
                    {
                        inputsR[i] = (float)bitmap.GetPixel(i % 32, i / 32).R / 255.0f;
                        inputsR[i] = (float)bitmap.GetPixel(i % 32, i / 32).G / 255.0f;
                        inputsR[i] = (float)bitmap.GetPixel(i % 32, i / 32).B / 255.0f;
                    }
                }

                if (iter == 0)
                    Console.WriteLine("Done Inputs");

                if (iter % 100 == 0)
                {
                    float errorR = brain.DoIteration(inputsR, inputsR);
                    float[] ResultR = brain.GetOutput();
                    brain.ClearInputs();

                    if (iter == 0)
                        Console.WriteLine("Done R Results");

                    float errorG = brain.DoIteration(inputsG, inputsG);
                    float[] ResultG = brain.GetOutput();
                    brain.ClearInputs();

                    if (iter == 0)
                        Console.WriteLine("Done G Results");

                    float errorB = brain.DoIteration(inputsB, inputsB);
                    float[] ResultB = brain.GetOutput();
                    brain.ClearInputs();

                    if (iter == 0)
                        Console.WriteLine("Done B Results");

                    float overallError = (errorR + errorG + errorB) / 3;
                    Console.WriteLine("Error at {0} is: {1}", iter, overallError);

                    using (Bitmap outBitmap = new Bitmap(32, 32))
                    {
                        for (int i = 0; i < 32*32; i++)
                        {
                            Color color = Color.FromArgb((int)(ResultR[i]*255), (int)(ResultG[i] * 255), (int)(ResultB[i] * 255));
                            outBitmap.SetPixel(i % 32, i / 32, color);
                        }
                        outBitmap.Save("out/" + Out++ + "_out.jpg");
                    }
                }
                else
                {
                    brain.DoIteration(inputsR, inputsR);
                    brain.ClearInputs();

                    brain.DoIteration(inputsG, inputsG);
                    brain.ClearInputs();

                    brain.DoIteration(inputsB, inputsB);
                    brain.ClearInputs();
                }
                
            }
            Console.ReadKey();
        }

    }
    
}
