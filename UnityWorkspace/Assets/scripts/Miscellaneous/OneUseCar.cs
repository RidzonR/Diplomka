using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnitySharpNEAT;
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
using BayatGames.SaveGameFree;

public class OneUseCar : MonoBehaviour
{

    public TextAsset brain;
    [SerializeField, Tooltip("The Unit Prefab, which inherits from UnitController, that should be evaluated and spawned.")]
    private UnitController _unitControllerPrefab = default;

    private NeatGenomeCustom genome;
    private NeatGenomeParametersCustom _neatGenomeParams;
    private NetworkActivationScheme _activationScheme = NetworkActivationScheme.CreateAcyclicScheme();
    private UnitController controller;
    private IBlackBox phenome;

    private bool infinite = false;

    public NeatSupervisorCustom.Trial[] trials;
    private int tvar = 0;

    private void Start()
    {
        foreach (NeatSupervisorCustom.Trial trial in trials)
        {
            trial.trialParrent.SetActive(false);
        }
        tvar = trials.Length;
        CreateUnit();
    }

    private void CreateUnit()
    {
        controller = Instantiate(_unitControllerPrefab, _unitControllerPrefab.transform.position, _unitControllerPrefab.transform.rotation,transform);
        genome = LoadChampion();
        if (genome != null)
        {
            phenome = new NeatGenomeDecoderCustom(_activationScheme).Decode(genome);
        }
        controller.ActivateUnit(phenome);
        controller.DeactivateUnit();
    }

    public void RestartUnit()
    {
        controller.DeactivateUnit();
        if (trials.Length != 0)
        {
            trials[(tvar - 1) % trials.Length].trialParrent.SetActive(false);
            trials[tvar % trials.Length].trialParrent.SetActive(true);
            foreach (KillWall killWall in trials[tvar % trials.Length].trialMovables)
            {
                killWall.StartCoroutine("NewGen");
            }
            tvar++;
        }
        controller.ActivateUnit(phenome);
    }

    public void DeactivateUnit()
    {
        trials[(tvar - 1) % trials.Length].trialParrent.SetActive(false);
        controller.DeactivateUnit();
    }

    public void ContinualRide()
    {
        if (infinite)
        {
            StopAllCoroutines();
            controller.DeactivateUnit();
            trials[(tvar - 1) % trials.Length].trialParrent.SetActive(false);
            infinite = false;
        }
        else
        {
            StartCoroutine("InfiniteRide");
            infinite = true;
        }
    }

    public IEnumerator InfiniteRide()
    {
        controller.DeactivateUnit();
        if (trials.Length != 0)
        {
            trials[(tvar - 1) % trials.Length].trialParrent.SetActive(false);
            trials[tvar % trials.Length].trialParrent.SetActive(true);
            foreach (KillWall killWall in trials[tvar % trials.Length].trialMovables)
            {
                killWall.StartCoroutine("NewGen");
            }
            tvar++;
        }
        controller.ActivateUnit(phenome);

        while (true)
        {
            yield return new WaitForSecondsRealtime(1f);
            if (!controller.IsActive)
            {
                controller.DeactivateUnit();
                if (trials.Length != 0)
                {
                    trials[(tvar - 1) % trials.Length].trialParrent.SetActive(false);
                    trials[tvar % trials.Length].trialParrent.SetActive(true);
                    foreach (KillWall killWall in trials[tvar % trials.Length].trialMovables)
                    {
                        killWall.StartCoroutine("NewGen");
                    }
                    tvar++;
                }
                controller.ActivateUnit(phenome);
            }
        }
    }

    public NeatGenomeCustom LoadChampion()
    {

        // try load file
        NeatSaveData saveData = null;
        if (brain != null)
        {
            //load pop
            SaveGame.Serializer = new BayatGames.SaveGameFree.Serializers.SaveGameJsonSerializer();
            //saveData = SaveGame.Load<NeatSaveData>(brain.text);
            saveData = JsonUtility.FromJson<NeatSaveData>(brain.text);

            List<NeatGenomeCustom> genomeList = null;
            _neatGenomeParams = new NeatGenomeParametersCustom(true);
            _neatGenomeParams.FeedforwardOnly = _activationScheme.AcyclicNetwork;
            NeatGenomeFactoryCustom genomeFactory = new NeatGenomeFactoryCustom(saveData.input, saveData.output, _neatGenomeParams);
            genomeFactory.InnovationIdGenerator.Reset();
            genomeFactory.GenomeIdGenerator.Reset();
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
                genomeList.Add(new NeatGenomeCustom(genomeFactory, genomeSave.genomeId, genomeSave.birthGen, nGeneList, cGeneList, saveData.input, saveData.output, true));

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
}



[CustomEditor(typeof(OneUseCar))]
public class OneUseCarEditor : Editor
{
    public override void OnInspectorGUI()
    {

        DrawDefaultInspector();

        OneUseCar myScript = (OneUseCar)target;
        if (GUILayout.Button("Restart Unit"))
        {
            myScript.RestartUnit();
        }
        if (GUILayout.Button("Deactivate Unit"))
        {
            myScript.DeactivateUnit();
        }
        if (GUILayout.Button("Infinite Ride"))
        {
            myScript.ContinualRide();
        }
    }
}