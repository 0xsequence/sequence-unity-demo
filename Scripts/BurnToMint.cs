using System;
using System.Numerics;
using System.Threading.Tasks;
using Sequence;
using Sequence.Contracts;
using Sequence.Provider;
using Sequence.Transactions;
using Sequence.Utils;
using Sequence.Wallet;
using UnityEngine;

namespace Game.Scripts
{
    public class BurnToMint
    {
        private SequenceEthClient _client;
        private Contract _contract;
        
        public BurnToMint()
        {
            _client = new SequenceEthClient(SequenceConnector.Chain);
            
            TextAsset abiJson = Resources.Load<TextAsset>("Contracts/ERC1155BurnToMint");
            string burnToMintAbi = abiJson.text;
            if (string.IsNullOrWhiteSpace(burnToMintAbi))
            {
                throw new Exception("Failed to read abi from file");
            }
            _contract = new Contract(SequenceConnector.BurnToMintContractAddress, burnToMintAbi);
        }
        
        public async Task<Tuple<BigInteger[], BigInteger[], BigInteger[], BigInteger[]>> GetMintingRequirements(string tokenId)
        {
            object[] mintingRequirementsResponse =
                await _contract.QueryContract<object[]>(
                    "getMintRequirements",
                    BigInteger.Parse(tokenId))(_client);
            Tuple<BigInteger[], BigInteger[], BigInteger[], BigInteger[]> mintingRequirements = new Tuple<BigInteger[], BigInteger[], BigInteger[], BigInteger[]>(
                mintingRequirementsResponse[0].ConvertToTArray<BigInteger, object>(),
                mintingRequirementsResponse[1].ConvertToTArray<BigInteger, object>(),
                mintingRequirementsResponse[2].ConvertToTArray<BigInteger, object>(),
                mintingRequirementsResponse[3].ConvertToTArray<BigInteger, object>());
            int costItems = mintingRequirements.Item1.Length;
            int requiredAchievements = mintingRequirements.Item3.Length;
            if (costItems != mintingRequirements.Item2.Length ||
                requiredAchievements != mintingRequirements.Item4.Length)
            {
                throw new Exception("Invalid minting requirements received from contract");
            }

            return mintingRequirements;
        }
        
        public async Task<TransactionReceipt> SetMintingRequirements(IWallet wallet, string tokenId, BigInteger[] burnTokenIds, BigInteger[] burnTokenAmounts, BigInteger[] holdTokenIds, BigInteger[] holdTokenAmounts)
        {
            EthTransaction transaction = await _contract.CallFunction("setMintRequirements",
                    BigInteger.Parse(tokenId), burnTokenIds, burnTokenAmounts, holdTokenIds, holdTokenAmounts)
                .Create(_client, new ContractCall(wallet.GetAddress()));
            TransactionReceipt receipt = await wallet.SendTransactionAndWaitForReceipt(_client, transaction);
            return receipt;
        }
    }
}