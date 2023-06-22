/*
------------------------------------------------------------------
  This file is part of UnitySharpNEAT 
  Copyright 2020, Florian Wolf
  https://github.com/flo-wolf/UnitySharpNEAT
------------------------------------------------------------------
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BayatGames.SaveGameFree;

namespace UnitySharpNEAT
{
    public class NeatUICustom : MonoBehaviour
    {
        /// <summary>
        /// Display simple Onscreen buttons for quickly accessing ceratain lifecycle funtions and to display generation info.
        /// </summary>
        [SerializeField]
        private NeatSupervisorCustom _neatSupervisor;

        private Canvas _cameraCanvas;

        [SerializeField]
        private TMPro.TextMeshProUGUI curGen;
        [SerializeField]
        private TMPro.TextMeshProUGUI bestFit;
        [SerializeField]
        private RectTransform NN;
        [SerializeField]
        private RectTransform inputParrent;
        [SerializeField]
        private RectTransform hiddenParrent;
        [SerializeField]
        private RectTransform outputParrent;
        [SerializeField]
        private GameObject neuron;
        [SerializeField]
        private RectTransform pathsParrent;
        [SerializeField]
        private GameObject path;
        [SerializeField]
        private TMPro.TMP_InputField experimentName;
        [SerializeField]
        private TMPro.TMP_InputField popSize;
        [SerializeField]
        private TMPro.TMP_InputField speciesCount;
        [SerializeField]
        private TMPro.TMP_InputField hiddenIF;
        [SerializeField]
        private Toggle fullyConnected;

        [SerializeField]
        private GameObject loadPrefab;
        [SerializeField]
        private RectTransform loadParrent;
        private List<GameObject> loadUIS = new List<GameObject>();

        private SharpNeat.Genomes.Neat.NeatGenomeCustom genome;
        private List<GameObject> inputNeurons = new List<GameObject>();
        private List<GameObject> hiddenNeurons = new List<GameObject>();
        private List<GameObject> outputNeurons = new List<GameObject>();
        private List<GameObject> paths = new List<GameObject>();

        private Dictionary<uint, RectTransform> idToPos = new Dictionary<uint, RectTransform>();

        [SerializeField]
        private GameObject loadUi;
        [SerializeField]
        private Button newExperimentButton;
        [SerializeField]
        private GameObject baseUI;

        private void LateUpdate()
        {
            curGen.text = _neatSupervisor.CurrentGeneration.ToString();
            bestFit.text = _neatSupervisor.CurrentBestFitness.ToString("N1");
        }

        private void Start()
        {
            _cameraCanvas = GetComponentInChildren<Canvas>();
            _cameraCanvas.worldCamera = Camera.main;

            hiddenIF.gameObject.SetActive(false);
        }

        public void StarEA()
        {
            _neatSupervisor.StartEvolution();
        }

        public void StopSaveEA()
        {
            _neatSupervisor.StopEvolution();
        }

        public void RunBest()
        {
            _neatSupervisor.RunBest();
        }
        
        public void ShowNN()
        {
            
            StartCoroutine("ReloadUI");

        }

        public void StopNN()
        {
            StopAllCoroutines();
        }

        public void NewExperiment()
        {
            Config config = new Config();
            config.experimentName = experimentName.text;
            config.popSize = int.Parse(popSize.text);
            config.speciesCount = int.Parse(speciesCount.text);
            config.fullyConnected = fullyConnected.isOn;
            if(fullyConnected.isOn)
                config.hiddenNeurons = int.Parse(hiddenIF.text);
            else
                config.hiddenNeurons = 0;
            _neatSupervisor.NewExperiment(config);

        }

        public void LoadExperiment()
        {
            foreach(GameObject temp in loadUIS)
            {
                Destroy(temp);
            }
            loadUIS.Clear();

            foreach(SaveFiles save in _neatSupervisor.saves)
            {
                GameObject temp = Instantiate(loadPrefab, loadParrent);
                LoadUI loadUI = temp.GetComponent<LoadUI>();
                if(save.input == _neatSupervisor.NetworkInputCount && save.output == _neatSupervisor.NetworkOutputCount)
                {
                    loadUI.LoadData(save,true, this);
                }
                else
                {
                    loadUI.LoadData(save, false, this);
                }

                loadUIS.Add(temp);
            }
            loadParrent.sizeDelta = new Vector2(0, _neatSupervisor.saves.Count*110+20);
        }

        public void ToggleChange()
        {
            if (fullyConnected.isOn)
            {
                hiddenIF.gameObject.SetActive(true);
            }
            else
            {
                hiddenIF.gameObject.SetActive(false);
            }
        }

        public IEnumerator ReloadUI()
        {
            while (true)
            {
                if(_neatSupervisor.EvolutionAlgorithm != null)
                    genome = _neatSupervisor.EvolutionAlgorithm.CurrentChampGenome;
                if (genome == null)
                    genome = _neatSupervisor.genome;
                if (genome == null)
                {
                    yield return new WaitForSecondsRealtime(1f);
                }
                else
                {

                    idToPos.Clear();
                    foreach (GameObject _gameObject in inputNeurons)
                    {
                        Destroy(_gameObject);
                    }
                    inputNeurons.Clear();
                    foreach (GameObject _gameObject in hiddenNeurons)
                    {
                        Destroy(_gameObject);
                    }
                    hiddenNeurons.Clear();
                    foreach (GameObject _gameObject in outputNeurons)
                    {
                        Destroy(_gameObject);
                    }
                    outputNeurons.Clear();
                    foreach (GameObject _gameObject in paths)
                    {
                        Destroy(_gameObject);
                    }
                    paths.Clear();

                    int i = 0, o = 0, h = 0;
                    float hiddens = 0;
                    foreach (SharpNeat.Genomes.Neat.NeuronGene neuronGene in genome.NeuronGeneList)
                    {
                        if(neuronGene.NodeType == SharpNeat.Network.NodeType.Hidden)
                        {
                            hiddens += 1f;
                        }
                    }
                    float inputSize = -inputParrent.sizeDelta.y / (_neatSupervisor.NetworkInputCount + 1);
                    float outputSize = -outputParrent.sizeDelta.y / (_neatSupervisor.NetworkOutputCount);
                    float hiddenSize = -hiddenParrent.sizeDelta.y / hiddens;

                    foreach (SharpNeat.Genomes.Neat.NeuronGene neuronGene in genome.NeuronGeneList)
                    {
                        if (neuronGene.NodeType == SharpNeat.Network.NodeType.Bias)
                        {
                            var temp = Instantiate(neuron, inputParrent);
                            var rect = temp.GetComponent<RectTransform>();
                            inputNeurons.Add(temp);
                            temp.GetComponent<NeuronUI>().SetColor(Color.white);
                            idToPos.Add(neuronGene.Id, rect);
                            rect.anchoredPosition = new Vector2(-218.9f, inputSize * i++ + inputSize / 2);
                        }
                        if (neuronGene.NodeType == SharpNeat.Network.NodeType.Input)
                        {
                            var temp = Instantiate(neuron, inputParrent);
                            var rect = temp.GetComponent<RectTransform>();
                            inputNeurons.Add(temp);
                            temp.GetComponent<NeuronUI>().SetColor(Color.red);
                            idToPos.Add(neuronGene.Id, rect);
                            rect.anchoredPosition = new Vector2(-218.9f, inputSize * i++ + inputSize / 2);
                        }
                        if (neuronGene.NodeType == SharpNeat.Network.NodeType.Output)
                        {
                            var temp = Instantiate(neuron, outputParrent);
                            var rect = temp.GetComponent<RectTransform>();
                            outputNeurons.Add(temp);
                            temp.GetComponent<NeuronUI>().SetColor(Color.blue);
                            idToPos.Add(neuronGene.Id, rect);
                            rect.anchoredPosition = new Vector2(218.9f, outputSize * o++ + outputSize / 2);
                        }
                        if (neuronGene.NodeType == SharpNeat.Network.NodeType.Hidden)
                        {
                            var temp = Instantiate(neuron, hiddenParrent);
                            var rect = temp.GetComponent<RectTransform>();
                            hiddenNeurons.Add(temp);
                            temp.GetComponent<NeuronUI>().SetColor(Color.green);
                            idToPos.Add(neuronGene.Id, rect);
                            rect.anchoredPosition = new Vector2(0f, hiddenSize * h++ + hiddenSize / 2);   
                        }
                    }

                    foreach (SharpNeat.Genomes.Neat.ConnectionGene conectionGene in genome.ConnectionGeneList)
                    {
                        //conectionGene.SourceNodeId;
                        var temp = Instantiate(path, pathsParrent);
                        paths.Add(temp);
                        var rect = temp.GetComponent<RectTransform>();
                        var rectTarget = idToPos[conectionGene.TargetNodeId];

                        rect.anchoredPosition = idToPos[conectionGene.SourceNodeId].anchoredPosition;

                        float distance = Vector2.Distance(rect.anchoredPosition, rectTarget.anchoredPosition);
                        float angle = Vector2.SignedAngle(Vector2.right, rectTarget.anchoredPosition - rect.anchoredPosition);

                        rect.sizeDelta = new Vector2(distance, rect.sizeDelta.y);
                        rect.localEulerAngles = new Vector3(0, 0, angle);

                        var lineUI = temp.GetComponent<LineUI>();
                        if(_neatSupervisor.activeConfig.fullyConnected)
                            lineUI.SetColor(Color.white, 0);
                        else
                            lineUI.SetColor(Color.white, (float)conectionGene.Weight);
                    }

                    yield return new WaitForSecondsRealtime(1f);
                }
            }

        }

        public void CheckSaveFileName()
        {
            bool match = false;
            foreach(SaveFiles save in _neatSupervisor.saves)
            {
                if (save.fileName == experimentName.text)
                    match = true;
            }
            if (match)
            {
                newExperimentButton.interactable = false;
                experimentName.caretColor = Color.red;
            }
            else
            {
                newExperimentButton.interactable = true;
                experimentName.caretColor = Color.white;
            }
        }

        public void LoadThis(SaveFiles saveToLoad)
        {
            loadUi.SetActive(false);
            Config config = new Config();
            config.experimentName = saveToLoad.fileName;
            config.popSize = saveToLoad.genomeCount;
            config.speciesCount = saveToLoad.speciesCount;
            config.hiddenNeurons = saveToLoad.hidden;
            
            if (saveToLoad.hidden == 0)
            {
                config.fullyConnected = false;
            }
            else
            {
                config.fullyConnected = true;
            }
            _neatSupervisor.NewExperiment(config);
            baseUI.SetActive(true);
        }

        public void DeleteThis(SaveFiles saveToDelete)
        {
            _neatSupervisor.saves.Remove(saveToDelete);
            SaveGame.Save<List<SaveFiles>>("saveFiles.DontTouch", _neatSupervisor.saves);
            SaveGame.Delete(saveToDelete.fileName + "Pop.json");
            SaveGame.Delete(saveToDelete.fileName + "Champ.json");
            LoadExperiment();
        }

    }

    [System.Serializable]
    public class Config
    {
        public string experimentName = "defaultName";
        public int popSize = 50;
        public int speciesCount = 10;
        public SharpNeat.Decoders.NetworkActivationScheme activationScheme = SharpNeat.Decoders.NetworkActivationScheme.CreateAcyclicScheme();
        public string complexityRegulationStrategy = "Absolute";
        public int complexityThreshold = 10;
        public bool fullyConnected;
        public int hiddenNeurons;
    }

}
