using SharpNeat.Core;
using SharpNeat.Decoders;
using SharpNeat.Decoders.Neat;
using SharpNeat.DistanceMetrics;
using SharpNeat.Domains;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.EvolutionAlgorithms.ComplexityRegulation;
using SharpNeat.Genomes.Neat;
using SharpNeat.Phenomes;
using SharpNeat.Network;
using SharpNeat.SpeciationStrategies;
using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using BayatGames.SaveGameFree;

namespace UnitySharpNEAT
{
    public class ExperimentCustom
    {
        #region MEMBER VARIABLES
        [SerializeField]
        private NeatEvolutionAlgorithmParameters _eaParams;

        [SerializeField]
        private NeatGenomeParametersCustom _neatGenomeParams;

        [SerializeField]
        private string _name;

        [SerializeField]
        private int _populationSize;

        [SerializeField]
        private int _specieCount;

        [SerializeField]
        private NetworkActivationScheme _activationScheme;

        [SerializeField]
        private string _complexityRegulationStr;

        [SerializeField]
        private int? _complexityThreshold;

        [SerializeField]
        private NeatSupervisorCustom _neatSupervisor;

        [SerializeField]
        private int _inputCount;

        [SerializeField]
        private int _outputCount;

        private bool _topologyAugument = true;
        private int _hiddenCount;
        private int _loadedGen = 0;

        #endregion

        #region PROPERTIES
        public string Name
        {
            get { return _name; }
        }

        public int InputCount
        {
            get { return _inputCount; }
        }

        public int OutputCount
        {
            get { return _outputCount; }
        }

        public int DefaultPopulationSize
        {
            get { return _populationSize; }
        }

        public NeatEvolutionAlgorithmParameters NeatEvolutionAlgorithmParameters
        {
            get { return _eaParams; }
        }

        public NeatGenomeParametersCustom NeatGenomeParameters
        {
            get { return _neatGenomeParams; }
        }
        #endregion

        #region FUNCTIONS
        public void Initialize(Config config, NeatSupervisorCustom neatSupervisor, int inputCount, int outputCount)
        {
            _name = config.experimentName;
            _populationSize = config.popSize;
            _specieCount = config.speciesCount;
            _activationScheme = config.activationScheme;
            _complexityRegulationStr = config.complexityRegulationStrategy;
            _complexityThreshold = config.complexityThreshold;

            _hiddenCount = 0;
            _topologyAugument = true;

            _eaParams = new NeatEvolutionAlgorithmParameters();
            _eaParams.SpecieCount = _specieCount;
            _neatGenomeParams = new NeatGenomeParametersCustom();
            _neatGenomeParams.FeedforwardOnly = _activationScheme.AcyclicNetwork;

            _neatSupervisor = neatSupervisor;

            _inputCount = inputCount;
            _outputCount = outputCount;
        }

        public void Initialize(Config config, NeatSupervisorCustom neatSupervisor, int inputCount, int outputCount, int hiddenCount)
        {
            _name = config.experimentName;
            _populationSize = config.popSize;
            _specieCount = config.speciesCount;
            _activationScheme = config.activationScheme;
            _complexityRegulationStr = config.complexityRegulationStrategy;
            _complexityThreshold = config.complexityThreshold;

            _hiddenCount = hiddenCount;
            _topologyAugument = false;

            _eaParams = new NeatEvolutionAlgorithmParameters();
            _eaParams.SpecieCount = _specieCount;
            _neatGenomeParams = new NeatGenomeParametersCustom(true);
            _neatGenomeParams.FeedforwardOnly = _activationScheme.AcyclicNetwork;

            _neatSupervisor = neatSupervisor;

            _inputCount = inputCount;
            _outputCount = outputCount;
        }

        public IGenomeDecoder<NeatGenomeCustom, IBlackBox> CreateGenomeDecoder()
        {

            return new NeatGenomeDecoderCustom(_activationScheme);
        }

        public IGenomeFactory<NeatGenomeCustom> CreateGenomeFactory()
        {
            return new NeatGenomeFactoryCustom(InputCount, OutputCount, _neatGenomeParams);
        }

        public NeatEvolutionAlgorithm<NeatGenomeCustom> CreateEvolutionAlgorithm(string fileName)
        {
            NeatGenomeFactoryCustom genomeFactory;
            List<NeatGenomeCustom> genomeList = LoadPopulation(out genomeFactory);
            return CreateEvolutionAlgorithm(genomeFactory, genomeList);
        }

        public NeatEvolutionAlgorithm<NeatGenomeCustom> CreateEvolutionAlgorithm()
        {
            return CreateEvolutionAlgorithm(_populationSize);
        }

        public NeatEvolutionAlgorithm<NeatGenomeCustom> CreateEvolutionAlgorithm(int populationSize)
        {
            // somewhere here add all conections

            IGenomeFactory<NeatGenomeCustom> genomeFactory = CreateGenomeFactory();

            List<NeatGenomeCustom> genomeList = genomeFactory.CreateGenomeList(populationSize, 0);

            return CreateEvolutionAlgorithm(genomeFactory, genomeList);
        }

        public NeatEvolutionAlgorithm<NeatGenomeCustom> CreateEvolutionAlgorithm(IGenomeFactory<NeatGenomeCustom> genomeFactory, List<NeatGenomeCustom> genomeList)
        {
            IDistanceMetric distanceMetric = new ManhattanDistanceMetric(1.0, 0.0, 10.0);
            ISpeciationStrategy<NeatGenomeCustom> speciationStrategy = new KMeansClusteringStrategy<NeatGenomeCustom>(distanceMetric);

            IComplexityRegulationStrategy complexityRegulationStrategy = ExperimentUtils.CreateComplexityRegulationStrategy(_complexityRegulationStr, _complexityThreshold);

            NeatEvolutionAlgorithm<NeatGenomeCustom> ea = new NeatEvolutionAlgorithm<NeatGenomeCustom>(_eaParams, speciationStrategy, complexityRegulationStrategy);

            // Create black box evaluator       
            BlackBoxFitnessEvaluatorCustom evaluator = new BlackBoxFitnessEvaluatorCustom(_neatSupervisor);

            IGenomeDecoder<NeatGenomeCustom, IBlackBox> genomeDecoder = CreateGenomeDecoder();

            IGenomeListEvaluator<NeatGenomeCustom> innerEvaluator = new CoroutinedListEvaluatorCustom<NeatGenomeCustom, IBlackBox>(genomeDecoder, evaluator, _neatSupervisor);

            IGenomeListEvaluator<NeatGenomeCustom> selectiveEvaluator = new SelectiveGenomeListEvaluator<NeatGenomeCustom>(innerEvaluator,
                SelectiveGenomeListEvaluator<NeatGenomeCustom>.CreatePredicate_OnceOnly());

            //ea.Initialize(selectiveEvaluator, genomeFactory, genomeList);
            ea.Initialize(innerEvaluator, genomeFactory, genomeList, _loadedGen);

            return ea;
        }
        #endregion

        #region POPULATION SAVING/LOADING
        /// <summary>
        /// Saves the specified genomes to the population safe file of this experiment (by default: myexperimentname.pop.xml)
        /// </summary>
        public bool SavePopulation(IList<NeatGenomeCustom> genomeList)
        {
            //update entries for loading
            
            bool overide = false;
            SaveFiles deleteThis = null;
            foreach(SaveFiles saveFile in _neatSupervisor.saves)
            {
                if(saveFile.fileName == _name)
                {
                    overide = true;
                    deleteThis = saveFile;
                }
            }
            if(overide == true)
            {
                _neatSupervisor.saves.Remove(deleteThis);
            }
            _neatSupervisor.saves.Add(new SaveFiles(_name,DateTime.Now,_inputCount,_outputCount,_hiddenCount,_specieCount,genomeList.Count));
            SaveGame.Save<List<SaveFiles>>("saveFiles.DontTouch", _neatSupervisor.saves);


            //save actual population
            NeatSaveData file = new NeatSaveData();
            file.input = _inputCount;
            file.output = _outputCount;
            file.topologyAugument = _topologyAugument;
            file.hidden = _hiddenCount;
            file.genomes = new List<GenomeSaveData>();

            foreach(NeatGenomeCustom genomeCustom in genomeList)
            {
                GenomeSaveData saveGenome = new GenomeSaveData();
                file.genomes.Add(saveGenome);
                saveGenome.birthGen = genomeCustom.BirthGeneration;
                saveGenome.genomeId = genomeCustom.Id;
                saveGenome.connList = new List<ConnectionListSD>();
                saveGenome.nodeList = new List<NodeListSD>();

                foreach (NeuronGene node in genomeCustom.NodeList)
                {
                    saveGenome.nodeList.Add(new NodeListSD(node.NodeType, node.Id, node.ActivationFnId, node.AuxState));
                }

                foreach (ConnectionGene conn in genomeCustom.ConnectionGeneList)
                {
                    saveGenome.connList.Add(new ConnectionListSD(conn.InnovationId, conn.SourceNodeId, conn.TargetNodeId, conn.Weight));
                }
            }

            file.generationCount = (int)_neatSupervisor.CurrentGeneration;

            SaveGame.Save<NeatSaveData>(_name + "Pop.json", file);
            Debug.Log(_name + "Pop.json was saved.");
            return true; //ExperimentIO.WritePopulation(this, genomeList);
        }

        /// <summary>
        /// Saves the specified genomes to the population safe file of this experiment (by default: myexperimentname.pop.xml)
        /// </summary>
        public bool SavePopulation(IList<NeatGenomeCustom> genomeList, string newName)
        {
            //update entries for loading

            bool overide = false;
            SaveFiles deleteThis = null;
            foreach (SaveFiles saveFile in _neatSupervisor.saves)
            {
                if (saveFile.fileName == newName)
                {
                    overide = true;
                    deleteThis = saveFile;
                }
            }
            if (overide == true)
            {
                _neatSupervisor.saves.Remove(deleteThis);
            }
            _neatSupervisor.saves.Add(new SaveFiles(newName, DateTime.Now, _inputCount, _outputCount, _hiddenCount, _specieCount, genomeList.Count));
            SaveGame.Save<List<SaveFiles>>("saveFiles.DontTouch", _neatSupervisor.saves);


            //save actual population
            NeatSaveData file = new NeatSaveData();
            file.input = _inputCount;
            file.output = _outputCount;
            file.topologyAugument = _topologyAugument;
            file.hidden = _hiddenCount;
            file.genomes = new List<GenomeSaveData>();

            foreach (NeatGenomeCustom genomeCustom in genomeList)
            {
                GenomeSaveData saveGenome = new GenomeSaveData();
                file.genomes.Add(saveGenome);
                saveGenome.birthGen = genomeCustom.BirthGeneration;
                saveGenome.genomeId = genomeCustom.Id;
                saveGenome.connList = new List<ConnectionListSD>();
                saveGenome.nodeList = new List<NodeListSD>();

                foreach (NeuronGene node in genomeCustom.NodeList)
                {
                    saveGenome.nodeList.Add(new NodeListSD(node.NodeType, node.Id, node.ActivationFnId, node.AuxState));
                }

                foreach (ConnectionGene conn in genomeCustom.ConnectionGeneList)
                {
                    saveGenome.connList.Add(new ConnectionListSD(conn.InnovationId, conn.SourceNodeId, conn.TargetNodeId, conn.Weight));
                }
            }

            file.generationCount = (int)_neatSupervisor.CurrentGeneration;

            SaveGame.Save<NeatSaveData>(newName + "Pop.json", file);
            Debug.Log(newName + "Pop.json was saved.");
            return true; //ExperimentIO.WritePopulation(this, genomeList);
        }

        /// <summary>
        /// Saves the specified genome to the champion safe file of this experiment (by default: myexperimentname.champ.xml)
        /// </summary>
        public bool SaveChampion(NeatGenomeCustom bestGenome)
        {          
            NeatSaveData file = new NeatSaveData();
            file.input = _inputCount;
            file.output = _outputCount;
            file.topologyAugument = _topologyAugument;
            file.hidden = _hiddenCount;
            file.generationCount = (int)_neatSupervisor.CurrentGeneration;
            file.genomes = new List<GenomeSaveData>();

            GenomeSaveData champion = new GenomeSaveData();
            file.genomes.Add(champion);
            champion.birthGen = bestGenome.BirthGeneration;
            champion.genomeId = bestGenome.Id;
            champion.connList = new List<ConnectionListSD>();
            champion.nodeList = new List<NodeListSD>();

            foreach (NeuronGene node in bestGenome.NodeList)
            {
                champion.nodeList.Add(new NodeListSD(node.NodeType, node.Id, node.ActivationFnId, node.AuxState));
            }
            
            foreach(ConnectionGene conn in bestGenome.ConnectionGeneList)
            {
                champion.connList.Add(new ConnectionListSD(conn.InnovationId, conn.SourceNodeId, conn.TargetNodeId, conn.Weight));
            }
            SaveGame.Serializer = new BayatGames.SaveGameFree.Serializers.SaveGameJsonSerializer();
            SaveGame.Save<NeatSaveData>(_name + "Champ.json", file);

            return true;  //ExperimentIO.WriteChampion(this, bestGenome);
        }

        /// <summary>
        /// Saves the specified genome to the champion safe file of this experiment (by default: myexperimentname.champ.xml)
        /// </summary>
        public bool SaveChampion(NeatGenomeCustom bestGenome, string newName)
        {
            NeatSaveData file = new NeatSaveData();
            file.input = _inputCount;
            file.output = _outputCount;
            file.topologyAugument = _topologyAugument;
            file.hidden = _hiddenCount;
            file.generationCount = (int)_neatSupervisor.CurrentGeneration;
            file.genomes = new List<GenomeSaveData>();

            GenomeSaveData champion = new GenomeSaveData();
            file.genomes.Add(champion);
            champion.birthGen = bestGenome.BirthGeneration;
            champion.genomeId = bestGenome.Id;
            champion.connList = new List<ConnectionListSD>();
            champion.nodeList = new List<NodeListSD>();

            foreach (NeuronGene node in bestGenome.NodeList)
            {
                champion.nodeList.Add(new NodeListSD(node.NodeType, node.Id, node.ActivationFnId, node.AuxState));
            }

            foreach (ConnectionGene conn in bestGenome.ConnectionGeneList)
            {
                champion.connList.Add(new ConnectionListSD(conn.InnovationId, conn.SourceNodeId, conn.TargetNodeId, conn.Weight));
            }
            SaveGame.Serializer = new BayatGames.SaveGameFree.Serializers.SaveGameJsonSerializer();
            SaveGame.Save<NeatSaveData>(newName + "Champ.json", file);

            return true;  //ExperimentIO.WriteChampion(this, bestGenome);
        }

        /// <summary>
        /// Loads the saved population from the population safe file of this experiment (by default: myexperimentname.pop.xml).
        /// If the file does not exist, then a new population is created and returned.
        /// </summary>
        public List<NeatGenomeCustom> LoadPopulation(out NeatGenomeFactoryCustom genomeFactory)
        {
            List<NeatGenomeCustom> genomeList = null;
            genomeFactory = new NeatGenomeFactoryCustom(InputCount, OutputCount, _neatGenomeParams, true);
            genomeFactory.InnovationIdGenerator.Reset();
            genomeFactory.GenomeIdGenerator.Reset();

            // try load file
            NeatSaveData saveData = null;
            if(SaveGame.Exists(_name + "Pop.json"))
            {
                //load pop
                saveData = SaveGame.Load<NeatSaveData>(_name + "Pop.json");

                _loadedGen = saveData.generationCount;
                //saveData.createSaveEachGen = -1;
                uint genomeId = 0, innoId = 0;
                genomeList = new List<NeatGenomeCustom>();
                foreach (GenomeSaveData genomeSave in saveData.genomes)
                {
                    NeuronGeneList nGeneList = new NeuronGeneList();
                    foreach(NodeListSD node in genomeSave.nodeList)
                    {
                        nGeneList.Add(new NeuronGene(node.id, node.neuronType, node.functionId, node.auxState));
                        if (innoId < node.id)
                            innoId = node.id;
                    }
                    ConnectionGeneList cGeneList = new ConnectionGeneList();
                    foreach(ConnectionListSD conn in genomeSave.connList)
                    {
                        cGeneList.Add(new ConnectionGene(conn.id, conn.srcId, conn.tgtId, conn.weight));
                        if (innoId < conn.id)
                            innoId = conn.id;
                    }
                    if (genomeSave.genomeId > genomeId)
                        genomeId = genomeSave.genomeId;
                    genomeList.Add(new NeatGenomeCustom(genomeFactory, genomeSave.genomeId, genomeSave.birthGen, nGeneList, cGeneList, _inputCount, _outputCount, true));

                }
                for(int i = 0; i <= genomeId; i++)
                    genomeFactory.NextGenomeId();
                for (int i = 0; i <= innoId; i++)
                    genomeFactory.NextInnovationId();
                return genomeList;
            }
            else
            {
                if(_hiddenCount == 0)
                {
                    //create normal pop
                    genomeList = genomeFactory.CreateGenomeList(DefaultPopulationSize, 0);
                    return genomeList;
                }
                else
                {
                    Debug.Log("created special pop");
                    //create special pop
                    genomeList = new List<NeatGenomeCustom>();
                    

                    NeuronGeneList nGeneList = new NeuronGeneList();
                    nGeneList.Add(new NeuronGene(genomeFactory.NextInnovationId(), NodeType.Bias, 0, null));
                    for (int i = 0; i < _inputCount; i++)
                    {
                        nGeneList.Add(new NeuronGene(genomeFactory.NextInnovationId(), NodeType.Input, 0, null));
                    }
                    for (int i = 0; i < _outputCount; i++)
                    {
                        nGeneList.Add(new NeuronGene(genomeFactory.NextInnovationId(), NodeType.Output, 0, null));
                    }
                    for (int i = 0; i<_hiddenCount;i++)
                    {
                        nGeneList.Add(new NeuronGene(genomeFactory.NextInnovationId(), NodeType.Hidden, 0, null));
                    }

                    for (int i = 0; i < _populationSize; i++)
                    {
                        ConnectionGeneList cGeneList = new ConnectionGeneList();
                        uint hiddenPading = (uint)(1 + _inputCount + _outputCount);

                        for (uint j = 0; j < _inputCount+1; j++)
                        {
                            for (uint k = 0; k < _hiddenCount; k++)
                            {
                                cGeneList.Add(new ConnectionGene(genomeFactory.NextInnovationId(), j, hiddenPading+k, GetRandomWeight()));
                            }
                        }
                        for (uint j = 0; j < _outputCount; j++)
                        {
                            for (uint k = 0; k < _hiddenCount; k++)
                            {
                                cGeneList.Add(new ConnectionGene(genomeFactory.NextInnovationId(), hiddenPading + k, j + (uint)(_inputCount + 1), GetRandomWeight()));
                            }
                        }
                        genomeList.Add(new NeatGenomeCustom(genomeFactory, genomeFactory.NextGenomeId(), 0, nGeneList, cGeneList, _inputCount, _outputCount, true));
                    }
                    return genomeList;
                }
            }
        }

        public double GetRandomWeight()
        {
            return UnityEngine.Random.Range((float)-NeatGenomeParameters.ConnectionWeightRange, (float)NeatGenomeParameters.ConnectionWeightRange);
        }

        /// <summary>
        /// Loads the saved champion genome from the champion safe file of this experiment (by default: myexperimentname.champ.xml).
        /// If the file does not exist, then null is returned
        /// </summary>
        public NeatGenomeCustom LoadChampion()
        {
            List<NeatGenomeCustom> genomeList = null;
            NeatGenomeFactoryCustom genomeFactory = (NeatGenomeFactoryCustom)CreateGenomeFactory();
            genomeFactory.InnovationIdGenerator.Reset();
            genomeFactory.GenomeIdGenerator.Reset();

            // try load file
            NeatSaveData saveData = null;
            if (SaveGame.Exists(_name + "Champ.json"))
            {
                //load pop
                saveData = SaveGame.Load<NeatSaveData>(_name + "Champ.json");
                uint genomeId = 0, innoId = 0;
                genomeList = new List<NeatGenomeCustom>();
                foreach (GenomeSaveData genomeSave in saveData.genomes)
                {
                    NeuronGeneList nGeneList = new NeuronGeneList();
                    foreach (NodeListSD node in genomeSave.nodeList)
                    {
                        nGeneList.Add(new NeuronGene(node.id, node.neuronType, node.functionId, node.auxState));
                        if (innoId < node.id)
                            innoId = node.id;
                    }
                    ConnectionGeneList cGeneList = new ConnectionGeneList();
                    foreach (ConnectionListSD conn in genomeSave.connList)
                    {
                        cGeneList.Add(new ConnectionGene(conn.id, conn.srcId, conn.tgtId, conn.weight));
                        if (innoId < conn.id)
                            innoId = conn.id;
                    }
                    if (genomeSave.genomeId > genomeId)
                        genomeId = genomeSave.genomeId;
                    genomeList.Add(new NeatGenomeCustom(genomeFactory, genomeSave.genomeId, genomeSave.birthGen, nGeneList, cGeneList, _inputCount, _outputCount, true));

                }
                for (int i = 0; i <= genomeId; i++)
                    genomeFactory.NextGenomeId();
                for (int i = 0; i <= innoId; i++)
                    genomeFactory.NextInnovationId();
                return genomeList[0];
            }
            else
            {
                
                return null;
                
            }
        }

        #endregion

    }

    [System.Serializable]
    public class NeatSaveData
    {
        public int input = 5;
        public int output = 1;

        public bool topologyAugument = true;
        public int hidden = 0;
        public List<GenomeSaveData> genomes;

        public int generationCount = 0;
    }

    [System.Serializable]
    public class GenomeSaveData
    {
        public uint genomeId;
        public uint birthGen;
        public uint fittnes;
        public List<NodeListSD> nodeList;
        public List<ConnectionListSD> connList;
        //new NeatGenomeCustom(null, genomeId, birthGen, nGeneList, cGeneList, inputNodeCount, outputNodeCount, true);
    }

    [System.Serializable]
    public class NodeListSD
    {
        public NodeType neuronType;
        public uint id;
        public int functionId = 0;
        public double[] auxState = null;

        public NodeListSD(NodeType neuronType, uint id, int functionId, double[] auxState)
        {
            this.neuronType = neuronType;
            this.id = id;
            this.functionId = functionId;
            this.auxState = auxState;
        }

        public NodeListSD()
        {
        }

        //hyper neat exclusive 

        //new NeuronGene(id, neuronType, functionId, auxState);
    }

    [System.Serializable]
    public class ConnectionListSD
    {
        public uint id;
        public uint srcId;
        public uint tgtId;
        public double weight;

        public ConnectionListSD(uint id, uint srcId, uint tgtId, double weight)
        {
            this.id = id;
            this.srcId = srcId;
            this.tgtId = tgtId;
            this.weight = weight;
        }

        public ConnectionListSD()
        {
        }


        //new ConnectionGene(id, srcId, tgtId, weight);
    }

    [System.Serializable]
    public class SaveFiles
    {
        public string fileName;
        public DateTime dateTime;
        public int input;
        public int output;
        public int hidden;
        public int speciesCount = 0;
        public int genomeCount = 0;

        public SaveFiles(string fileName, DateTime dateTime, int input, int output,int hidden,int speciesCount, int genomeCount)
        {
            this.fileName = fileName;
            this.dateTime = dateTime;
            this.input = input;
            this.output = output;
            this.hidden = hidden;
            this.speciesCount = speciesCount;
            this.genomeCount = genomeCount;
        }

        public SaveFiles()
        {
        }
    }
}
