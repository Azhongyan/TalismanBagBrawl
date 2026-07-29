using System;
using UnityEngine;

namespace TalismanBag.EditorTools.ItemBalance
{
    public static class ItemFourCoreCandidateDataCorrectionVerifier
    {
        public static void VerifyStaticBatch()
        {
            ItemFourCoreCandidateDataCorrectionSupport.VerificationEvidence evidence =
                ItemFourCoreCandidateDataCorrectionSupport.VerifyFinalStateFromDisk();
            if (evidence.errors.Count > 0)
                throw new InvalidOperationException(
                    "ITEM_FOUR_CORE_CANDIDATE_DATA_CORRECTION01_FAIL\n"
                    + string.Join("\n", evidence.errors));
            Debug.Log(ItemFourCoreCandidateDataCorrectionSupport.MigrationPassSummary);
        }
    }
}
