using SharpNeat.Core;
using SharpNeat.Genomes.Neat;
using SharpNeat.Phenomes;
using SharpNeat.Phenomes.NeuralNets;
using System;
using UnityEngine;

namespace SharpNeat.Decoders.Neat
{
    /// <summary>
    /// Decodes NeatGenomeCustom's into concrete network instances.
    /// </summary>
    /// 

    [Serializable]
    public class NeatGenomeDecoderCustom : IGenomeDecoder<NeatGenomeCustom, IBlackBox>
    {
        [SerializeField] readonly NetworkActivationScheme _activationScheme;
        delegate IBlackBox DecodeGenome(NeatGenomeCustom genome);
        [SerializeField] readonly DecodeGenome _decodeMethod;

        #region Constructors

        /// <summary>
        /// Construct the decoder with the network activation scheme to use in decoded networks.
        /// </summary>
        public NeatGenomeDecoderCustom(NetworkActivationScheme activationScheme)
        {
            _activationScheme = activationScheme;

            // Pre-determine which decode routine to use based on the activation scheme.
            _decodeMethod = GetDecodeMethod(activationScheme);
        }

        #endregion

        #region IGenomeDecoder Members

        /// <summary>
        /// Decodes a NeatGenomeCustom to a concrete network instance.
        /// </summary>
        public IBlackBox Decode(NeatGenomeCustom genome)
        {
            return _decodeMethod(genome);
        }

        #endregion

        #region Private Methods

        private DecodeGenome GetDecodeMethod(NetworkActivationScheme activationScheme)
        {
            if (activationScheme.AcyclicNetwork)
            {
                return DecodeToFastAcyclicNetwork;
            }

            if (activationScheme.FastFlag)
            {
                return DecodeToFastCyclicNetwork;
            }
            return DecodeToCyclicNetwork;
        }

        private FastAcyclicNetwork DecodeToFastAcyclicNetwork(NeatGenomeCustom genome)
        {
            return FastAcyclicNetworkFactory.CreateFastAcyclicNetwork(genome);
        }

        private CyclicNetwork DecodeToCyclicNetwork(NeatGenomeCustom genome)
        {
            return CyclicNetworkFactory.CreateCyclicNetwork(genome, _activationScheme);
        }

        private FastCyclicNetwork DecodeToFastCyclicNetwork(NeatGenomeCustom genome)
        {
            return FastCyclicNetworkFactory.CreateFastCyclicNetwork(genome, _activationScheme);
        }

        #endregion
    }
}
