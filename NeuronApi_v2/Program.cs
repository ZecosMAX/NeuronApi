using System;
using System.Collections.Generic;
using System.Linq;

namespace NeuronApi_v2
{
    class Program {
        static void Main(string[] args)
        {
            int InputsCount = 2;

            int[] HiddensCount = { 2, 2 };

            int OutputsCount = 1;

            Brain brain = new Brain(InputsCount, OutputsCount, HiddensCount);

            float[] inputs = { -0.4534345f, 0.853431287f };
            float[] perfects = { 0.2f };

            //float[] inputs = {
            //    8.0f / 8.0f,
            //    7.0f / 8.0f,
            //    6.0f / 8.0f,
            //    5.0f / 8.0f,
            //    4.0f / 8.0f,
            //    3.0f / 8.0f,
            //    2.0f / 8.0f,
            //    1.0f / 8.0f };

            brain.SetInput(inputs);
            float error = brain.DoIteration(perfects);
            //brain.CheckInputOfOutputs();
            //brain.DrawOutputs();
            Console.WriteLine("Outputs is: ");
            Console.WriteLine("\t" + string.Join("\n\t", brain.GetOutput()));

            Console.WriteLine("\nError is: {0}%", error * 100);

            brain.DoTraining(perfects);

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
        private float InputValue;
        private float OutputValue;

        Func<float, float> CustomActivation;
        private bool isCustomFunction = false;

        public Neuron()
        {
            this.type = "neuron";
        }
        private static float Activation(float x)
        {
            return 1.0f / (1 + MathF.Pow(MathF.E, x));
        }
        public static float Derrivative(float x)
        {
            return (1.0f - x) * x;
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

        private float WeightValue;

        public Synapse(Neuron s, Neuron d)
        {
            this.Source = s;
            this.Destination = d;
            WeightValue = (float)new Random().NextDouble();
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

        int Inputs;
        float InputBiasValue;
        int[] Hiddens;
        float[] HiddensBiasValue;
        int Outputs;

        public Brain(int InputCount, int OutputCount, int[] HiddensCount)
        {
            this.Inputs = InputCount;
            this.Hiddens = HiddensCount;
            this.Outputs = OutputCount;

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
                SetInput(inputs);
            }

            for (int i = 0; i < Synapses.Count; i++)
            {
                Synapses[i].TransferValue();
            }
            //int firstSynapsesCount = Inputs * Hiddens[0];
            //for (int i = 0; i < firstSynapsesCount; i++)
            //{
            //    Synapses[i].TransferValue();
            //}
            //int hiddenSynapsesProceed = 0;
            //for (int i = 0; i < Hiddens.Length - 1; i++)
            //{
            //    int HS = Hiddens[i] * Hiddens[i + 1];
            //    hiddenSynapsesProceed += HS;

            //    Synapses[firstSynapsesCount + i].TransferValue();
            //}

            //int lastSynapsesCount = Outputs * Hiddens[Hiddens.Length - 1];
            //for (int i = 0; i < lastSynapsesCount; i++)
            //{
            //    Synapses[firstSynapsesCount + hiddenSynapsesProceed + i].TransferValue();
            //}
            return this.GetIterationError(perfect);
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
                if(neuron.GetNeuronType() != "biasneuron")
                    Console.WriteLine("Output of an neuron is {0}", neuron.GetOutput());
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
            for (int i = Hiddens.Length; i > 0; i--)
            {
                Neuron[] iThLayer = Neurons.Where((n) => { return n.GetNeuronType().Contains("hiddenneuron_" + (i - 1)); }).ToArray();

                for (int j = 0; j < iThLayer.Length; j++)
                {
                    Synapse[] synapses = Synapses.Where((s) => { return s.GetSource().Equals(iThLayer[i]); }).ToArray();
                    for (int k = 0; k < synapses.Length; k++)
                    {
                        if(i == Hiddens.Length)
                        {
                            synapses[k].GetWeight() //остановился на моменте, где нужно подсчитать дельту первого слоя скрытых нейронов
                        }
                    }
                }

                Console.WriteLine("Trained {0}th layer, size: {1} ", i, iThLayer.Length);
            }
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
