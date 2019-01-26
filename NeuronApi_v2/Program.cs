using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NeuronApi_v2
{
    class Program {
        public static readonly Random random = new Random();
        static void Main(string[] args)
        {
            int InputsCount = 2;
            int[] HiddensCount = { 2 };
            int OutputsCount = 1;
            float Epsilon = 0.01f;
            float Alpha = 0.01f;

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
                int first = random.Next(0, 2);
                int second = random.Next(0, 2);

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
            while(true)
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

    class BiasNeuron : Neuron
    {
        private float BiasOutput;

        public BiasNeuron(float o)
        {
            this.type = "biasneuron";
            this.BiasOutput = o;
        }
        public void SetBiasOutput(float o)
        {
            this.BiasOutput = o;
        }
        public float GetBiasOutput()
        {
            return 1.0f;
        }
    }
    class Neuron
    {
        protected string type;
        protected float InputValue;
        protected float OutputValue;
        protected float Delta;

        protected Func<float, float> CustomActivation;
        protected bool isCustomFunction = false;

        public Neuron()
        {
            this.type = "neuron";
        }
        protected static float Activation(float x)
        {
            return 1.0f / (1.0f + MathF.Pow(MathF.E, -x));
        }
        public static float Derrivative(float x)
        {
            return (1.0f - Activation(x)) * Activation(x);
        }
        public void SetDelta(float delta)
        {
            this.Delta = delta;
        }
        public float GetDelta()
        {
            return this.Delta;
        }
        
        public void SetCustomActivation(Func<float, float> func)
        {
            CustomActivation = func;
            isCustomFunction = true;
        }
        public void SetInput(float input)
        {
            InputValue += input;
        }
        public float GetOutput()
        {
            if(this.type == "inputneuron")
            {
                return InputValue;
            }
            if (isCustomFunction)
            {
                OutputValue = CustomActivation(InputValue);
            }
            else
            {
                OutputValue = Activation(InputValue);
            }
            return OutputValue;
        }
        public float GetInput()
        {
            return InputValue;
        }
        public void ClearInput()
        {
            this.InputValue = 0;
        }
        public void SetNeuronType(string s)
        {
            this.type = s;
        }
        public string GetNeuronType()
        {
            return type;
        }
    }
    class Synapse
    {
        private readonly Neuron Source;
        private readonly Neuron Destination;

        private float PreviousDeltaWeight = 0.0f;
        private float WeightValue;

        public Synapse(Neuron s, Neuron d)
        {
            this.Source = s;
            this.Destination = d;
            WeightValue = (float)Program.random.NextDouble();
            Console.WriteLine("Weight Value is: {0}", WeightValue);
        }
        public Neuron GetDest()
        {
            return Destination;
        }
        public Neuron GetSource()
        {
            return Source;
        }
        public float GetPreviousDelta()
        {
            return PreviousDeltaWeight;
        }
        public void SetPreviousDelta(float val)
        {
            this.PreviousDeltaWeight = val;
        }
        public void TransferValue()
        {
            if (Destination.GetNeuronType().Contains("biasneuron"))
            {

            }
            else
            {
                if (Source.GetNeuronType().Contains("biasneuron"))
                {
                    Destination.SetInput(WeightValue);
                }
                else
                {
                    Destination.SetInput(Source.GetOutput() * WeightValue);
                }
            }
        }
        public void SetWeight(float w)
        {
            //Console.WriteLine("Preivous weight: {0}\nNew weight: {1}", WeightValue, w);
            WeightValue = w;
        }
        public float GetWeight()
        {
            return WeightValue;
        }
    }
    class Brain
    {
        List<Neuron> Neurons = new List<Neuron>();
        List<Synapse> Synapses = new List<Synapse>();

        float Epsilon = 0; //Speed of learning
        float Alpha = 0; //Weight momentum

        int Inputs;
        float InputBiasValue;
        int[] Hiddens;
        float[] HiddensBiasValue;
        int Outputs;

        public Brain(int InputCount, int OutputCount, int[] HiddensCount, float e, float a)
        {
            this.Inputs = InputCount;
            this.Hiddens = HiddensCount;
            this.Outputs = OutputCount;

            this.Epsilon = e;
            this.Alpha = a;

            //adding input neurons
            //last one is the bias
            for (int i = 0; i < InputCount + 1; i++)
            {
                Neuron input = new Neuron();
                input.SetNeuronType("inputneuron");
                Console.Write("Adding a input neuron ");
                if (i == InputCount)
                {
                    input = new BiasNeuron(0);
                    input.SetNeuronType("biasneuron_inputneuron");
                    Console.Write("(bias) ");
                }
                Console.Write(Neurons.Count);
                Console.WriteLine();

                Neurons.Add(input);
            }
            Console.WriteLine("-----------------------------------");
            //adding layers of hidden neurons and binding them with synapses
            for (int i = 0; i < HiddensCount.Length; i++)
            {
                for (int j = 0; j < HiddensCount[i] + 1; j++)
                {
                    Neuron HiddenNeuron = new Neuron();
                    HiddenNeuron.SetNeuronType("hiddenneuron_" + i);
                    Console.Write("Adding a hidden neuron in {0}-th layer ", i);
                    if (j == HiddensCount[i])
                    {
                        HiddenNeuron = new BiasNeuron(0);
                        HiddenNeuron.SetNeuronType("biasneuron_hiddenneuron_" + i);
                        Console.Write("(bias) ");
                    }
                    Console.Write(Neurons.Count);
                    Console.WriteLine();

                    if (i == 0)
                    {
                        for (int z = 0; z < InputCount + 1; z++)
                        {
                            Synapse synapse = new Synapse(Neurons[z], HiddenNeuron);
                            Console.Write(" ---> Binding input Neuron at {0} to current\n", z);
                            Synapses.Add(synapse);
                        }
                    }
                    else
                    {
                        for (int z = 0; z < HiddensCount[i - 1] + 1; z++)
                        {
                            int indexOffset = (InputCount + 1);
                            for (int w = 0; w < i - 1; w++)
                            {
                                indexOffset += (HiddensCount[w] + 1);
                            }
                            Console.Write(" ---> Binding input Neuron at {0} to current\n", indexOffset + z);
                            Synapse synapse = new Synapse(Neurons[indexOffset + z], HiddenNeuron);
                            Synapses.Add(synapse);
                        }
                    }
                    Neurons.Add(HiddenNeuron);
                }
            }
            Console.WriteLine("-----------------------------------");
            //adding final output layer
            for (int i = 0; i < OutputCount; i++)
            {
                Neuron OutputNeuron = new Neuron();
                OutputNeuron.SetNeuronType("outputneuron");
                Console.WriteLine("Adding a output neuron");
                for (int j = 0; j < HiddensCount[HiddensCount.Length - 1] + 1; j++)
                {
                    int indexOffset = (InputCount + 1);
                    for (int w = 0; w < HiddensCount.Length - 1; w++)
                    {
                        indexOffset += (HiddensCount[w] + 1);
                    }
                    Console.Write(" ---> Binding input Neuron at {0} to current\n", indexOffset + j);
                    Synapse synapse = new Synapse(Neurons[indexOffset + j], OutputNeuron);
                    Synapses.Add(synapse);
                }
                Neurons.Add(OutputNeuron);
            }

            Console.WriteLine("\nTotal neurons: {0}\nTotal synapses: {1}", Neurons.Count, Synapses.Count);
        }
        public void SetInput(float[] inputs)
        {
            if(inputs.Length != Inputs)
            {
                throw new Exception("Inputs vals size aren't match");
            }
            for (int i = 0; i < Inputs; i++)
            {
                Neurons[i].SetInput(inputs[i]);
            }
        }
        public float DoIteration(float[] perfect, float[] inputs = null)
        {
            if(inputs != null)
            {
                //Console.WriteLine(true);
                SetInput(inputs);
            }
            for (int i = 0; i < Synapses.Count; i++)
            {
                Synapses[i].TransferValue();
            }

            float error = this.GetIterationError(perfect);
            this.DoTraining(perfect);

            return error;
        }
        public float DoProbe(float[] perfect, float[] inputs = null)
        {
            if (inputs != null)
            {
                //Console.WriteLine(true);
                SetInput(inputs);
            }
            for (int i = 0; i < Synapses.Count; i++)
            {
                Synapses[i].TransferValue();
            }

            float error = this.GetIterationError(perfect);

            return error;
        }
        public void CheckInputOfOutputs()
        {
            for (int i = 0; i < Outputs; i++)
            {
                Console.WriteLine("\t" + Neurons[Neurons.Count - Outputs + i].GetInput());
            }
        }
        public void DrawOutputs()
        {
            foreach (var neuron in Neurons)
            {
                if(!neuron.GetNeuronType().Contains("biasneuron"))
                    Console.WriteLine("Output of an neuron ({1}) is {0} and his input: {2}\t\t", neuron.GetOutput(), neuron.GetNeuronType(), neuron.GetInput());
                else
                    Console.WriteLine("Output of an bias neuron is {0}", (neuron as BiasNeuron).GetBiasOutput());
            }
        }
        public void ClearInputs()
        {
            foreach (var neuron in Neurons)
            {
                neuron.ClearInput(); //Because of architecute, i need to clean all input values after performing an iteration
            }
        }
        public float GetIterationError(float[] perfect)
        {
            if (perfect.Length != Outputs)
            {
                throw new Exception("Perfect values array size is not matches with output neurons count");
            }
            float error = 0.0f;
            float[] outputs = this.GetOutput();

            for (int i = 0; i < Outputs; i++)
            {
                error += MathF.Pow(perfect[i] - outputs[i], 2.0f);
            }

            error /= Outputs;

            return error;
        }
        public void DoTraining(float[] perfect)
        {
            float[] OutputDeltas = GetDeltaO(perfect);
            Neuron[] previousLayer = null;
            for (int i = Hiddens.Length; i > 0; i--)
            {
                Neuron[] iThLayer = Neurons.Where((n) => { return n.GetNeuronType().Contains("hiddenneuron_" + (i - 1)); }).ToArray();

                for (int j = 0; j < iThLayer.Length; j++)
                {
                    Synapse[] synapses = Synapses.Where((s) => { return s.GetSource().Equals(iThLayer[j]); }).ToArray();
                    float wSum = 0;
                    if (i == Hiddens.Length) //this means this is the last hidden layer
                    {
                        for (int k = 0; k < synapses.Length; k++)
                        {
                            wSum += synapses[k].GetWeight() * OutputDeltas[k];
                        }
                        float currentNeuronDelta = Neuron.Derrivative(iThLayer[j].GetOutput()) * wSum; //Calculated delta for a neuron
                        iThLayer[j].SetDelta(currentNeuronDelta);

                        //Starting to update all the synapses, which is originated from this neuron
                        for (int k = 0; k < synapses.Length; k++)
                        {
                            //Synapse gradient is output of neuron at the start of synapse multiplyed by delta of a neuron at the end of synapse
                            float synapseGradient = synapses[k].GetSource().GetOutput() * OutputDeltas[k]; 
                            float deltaWeight = Epsilon * synapseGradient + Alpha * synapses[k].GetPreviousDelta(); //DeltaW = E*Grad + A*(Previous change)
                            synapses[k].SetPreviousDelta(deltaWeight);
                            synapses[k].SetWeight(synapses[k].GetWeight() + deltaWeight);
                        }
                    }
                    else
                    {
                        for (int k = 0; k < synapses.Length; k++) //also, count of synapses, originated from this neuron @ iThLayer[j] is equal to count of neurons of next layer (previous in this context)
                        {
                            wSum += synapses[k].GetWeight() * previousLayer[k].GetDelta(); //good thing, that i saved previous layer into array to get deltas
                        }
                        float currentNeuronDelta = Neuron.Derrivative(iThLayer[j].GetOutput()) * wSum;
                        iThLayer[j].SetDelta(currentNeuronDelta);

                        //Starting to update all the synapses, which is originated from this neuron
                        for (int k = 0; k < synapses.Length; k++)
                        {
                            //Synapse gradient is output of neuron at the start of synapse multiplyed by delta of a neuron at the end of synapse
                            float synapseGradient = synapses[k].GetSource().GetOutput() * previousLayer[k].GetDelta();
                            float deltaWeight = Epsilon * synapseGradient + Alpha * synapses[k].GetPreviousDelta(); //DeltaW = E*Grad + A*(Previous change)
                            synapses[k].SetPreviousDelta(deltaWeight);
                            synapses[k].SetWeight(synapses[k].GetWeight() + deltaWeight);
                        }
                    }
                }
                previousLayer = iThLayer;
                Console.WriteLine("Trained {0}th HIDDEN layer, size: {1} ", i, iThLayer.Length);
                
            }
            //at this point, previousLayer is array, which contains 1st layer of hidden section, so we can do the same thing for input neurons
            Neuron[] inputLayer = Neurons.Where((n) => { return n.GetNeuronType().Contains("inputneuron"); }).ToArray();
            for (int i = 0; i < inputLayer.Length; i++)
            {
                Synapse[] synapses = Synapses.Where((s) => { return s.GetSource().Equals(inputLayer[i]); }).ToArray();
                

                //we don't really need to set delta of input layer, like, at all
                //because input layer has no synapses "behind" it, and so delta won't be used

                for (int j = 0; j < synapses.Length; j++)
                { 
                    float synapseGradient = synapses[j].GetSource().GetOutput() * previousLayer[j].GetDelta(); //Calculated gradient
                    float deltaWeight = Epsilon * synapseGradient + Alpha * synapses[j].GetPreviousDelta(); //DeltaW = E*Grad + A*(Previous change)
                    synapses[j].SetPreviousDelta(deltaWeight);
                    synapses[j].SetWeight(synapses[j].GetWeight() + deltaWeight);
                }
            }
            Console.WriteLine("Trained input layer");
        }
        private float[] GetDeltaO(float[] perfect)
        {
            if (perfect.Length != Outputs)
            {
                throw new Exception("Perfect values array size is not matches with output neurons count");
            }
            float[] outputs = this.GetOutput();
            float[] result = new float[Outputs];

            for (int i = 0; i < Outputs; i++)
            {
                result[i] = (perfect[i] - outputs[i]) * Neuron.Derrivative(outputs[i]);
            }
            return result;
        }
        public float[] GetOutput()
        {
            return Neurons.Where((n) => { return n.GetNeuronType() == "outputneuron"; }).Select((n) => { return n.GetOutput(); }).ToArray();
        }
    }
}
