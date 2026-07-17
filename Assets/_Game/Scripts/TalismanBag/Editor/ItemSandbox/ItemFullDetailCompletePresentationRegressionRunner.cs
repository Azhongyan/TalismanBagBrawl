using System;
using TalismanBag.EditorTools.ItemBalance;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.ItemSandbox
{
    public static class ItemFullDetailCompletePresentationRegressionRunner
    {
        public const string Marker = "ITEM_FULL_DETAIL_COMPLETE_PRESENTATION01_REGRESSION_PASS";

        public static void RunBatch()
        {
            try
            {
                // Executes the historical verifier code in this invocation; it does not
                // infer success from old report timestamps.
                ItemFullDetailBuildSandboxWorkbenchRegressionRunner.RunBatch();
                ItemBalanceWorkbenchVerifier.RunVerification();
                ItemBalanceCandidateDetailSandboxAdapterVerifier.VerifyMenu();
                ItemFullDetailBuildSandboxWorkbenchVerifier.VerifyMenu();
                Debug.Log(Marker);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }
    }
}
