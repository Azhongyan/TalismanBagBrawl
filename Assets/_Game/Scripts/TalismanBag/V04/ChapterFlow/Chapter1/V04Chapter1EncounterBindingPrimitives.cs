using System;
using System.Collections.Generic;

namespace TalismanBag.V04.ChapterFlow.Chapter1
{
    public sealed class V04Chapter1EncounterBindingDefinition
    {
        public readonly string schemaId;
        public readonly string chapterId;
        public readonly string stageId;
        public readonly string chapterFlowSlotId;
        public readonly string encounterNodeId;
        public readonly string activeEnemyContentId;
        public readonly string bindingPurpose;
        public readonly string compositionMode;
        public readonly string runtimeBindingStatus;
        public readonly string futureReplacementSlotId;
        public readonly string futureReplacementStatus;
        public readonly string decisionDependency;
        public readonly bool devOnly;
        public readonly bool isEnabled;
        public readonly bool formalFlow;

        public V04Chapter1EncounterBindingDefinition(
            string schemaId,
            string chapterId,
            string stageId,
            string chapterFlowSlotId,
            string encounterNodeId,
            string activeEnemyContentId,
            string bindingPurpose,
            string compositionMode,
            string runtimeBindingStatus,
            string futureReplacementSlotId,
            string futureReplacementStatus,
            string decisionDependency,
            bool devOnly,
            bool isEnabled,
            bool formalFlow)
        {
            this.schemaId = schemaId ?? string.Empty;
            this.chapterId = chapterId ?? string.Empty;
            this.stageId = stageId ?? string.Empty;
            this.chapterFlowSlotId = chapterFlowSlotId ?? string.Empty;
            this.encounterNodeId = encounterNodeId ?? string.Empty;
            this.activeEnemyContentId = activeEnemyContentId ?? string.Empty;
            this.bindingPurpose = bindingPurpose ?? string.Empty;
            this.compositionMode = compositionMode ?? string.Empty;
            this.runtimeBindingStatus = runtimeBindingStatus ?? string.Empty;
            this.futureReplacementSlotId = futureReplacementSlotId ?? string.Empty;
            this.futureReplacementStatus = futureReplacementStatus ?? string.Empty;
            this.decisionDependency = decisionDependency ?? string.Empty;
            this.devOnly = devOnly;
            this.isEnabled = isEnabled;
            this.formalFlow = formalFlow;
        }
    }

    public sealed class V04Chapter1HeldContentDefinition
    {
        public readonly string heldContentId;
        public readonly string heldDisplayIdentity;
        public readonly string preferredChapterFlowSlotId;
        public readonly string heldSlotId;
        public readonly string status;
        public readonly string decisionDependency;
        public readonly bool activeRuntimeBinding;
        public readonly bool activeStageBinding;
        public readonly bool copyScopeAuthored;
        public readonly bool devOnly;
        public readonly bool isEnabled;
        public readonly bool formalFlow;

        public V04Chapter1HeldContentDefinition(
            string heldContentId,
            string heldDisplayIdentity,
            string preferredChapterFlowSlotId,
            string heldSlotId,
            string status,
            string decisionDependency,
            bool activeRuntimeBinding,
            bool activeStageBinding,
            bool copyScopeAuthored,
            bool devOnly,
            bool isEnabled,
            bool formalFlow)
        {
            this.heldContentId = heldContentId ?? string.Empty;
            this.heldDisplayIdentity = heldDisplayIdentity ?? string.Empty;
            this.preferredChapterFlowSlotId = preferredChapterFlowSlotId ?? string.Empty;
            this.heldSlotId = heldSlotId ?? string.Empty;
            this.status = status ?? string.Empty;
            this.decisionDependency = decisionDependency ?? string.Empty;
            this.activeRuntimeBinding = activeRuntimeBinding;
            this.activeStageBinding = activeStageBinding;
            this.copyScopeAuthored = copyScopeAuthored;
            this.devOnly = devOnly;
            this.isEnabled = isEnabled;
            this.formalFlow = formalFlow;
        }
    }

    [Serializable]
    public sealed class V04Chapter1EncounterBindingValidationIssue
    {
        public string assertionId = string.Empty;
        public string category = string.Empty;
        public string expected = string.Empty;
        public string actual = string.Empty;
        public string message = string.Empty;
    }

    [Serializable]
    public sealed class V04Chapter1EncounterBindingValidationResult
    {
        public List<V04Chapter1EncounterBindingValidationIssue> issues = new();

        public bool Passed => issues.Count == 0;

        public void Add(
            string assertionId,
            string category,
            string expected,
            string actual,
            string message)
        {
            issues.Add(new V04Chapter1EncounterBindingValidationIssue
            {
                assertionId = assertionId ?? string.Empty,
                category = category ?? string.Empty,
                expected = expected ?? string.Empty,
                actual = actual ?? string.Empty,
                message = message ?? string.Empty
            });
        }
    }
}
