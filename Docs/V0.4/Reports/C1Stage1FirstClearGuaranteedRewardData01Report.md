# C1 Stage 1 First Clear Guaranteed Reward Data 01 Report

- Package: `V0.4-C1Stage1FirstClearGuaranteedRewardData01`
- Status: `DEV_COMPLETE / QA_PASS / USER_HANDTEST_NOT_APPLICABLE`
- Terminal marker: `C1_STAGE1_FIRST_CLEAR_GUARANTEED_REWARD_DATA01_PASS`
- Assertions: `90 total / 0 failed`
- Assignment SHA before: `cd0c1492d9319ff258e5f3725182a8c1294b4b8ccc97c33b07ac7a0be2ee76b0`
- Assignment SHA after: `cd0c1492d9319ff258e5f3725182a8c1294b4b8ccc97c33b07ac7a0be2ee76b0`

## Canonical Data

- Data: `CAMPAIGN_NORMAL_LV1 / bone_aspect_chapter_1 / 1-1 / FirstClearFixed / I002@white / quantity=1`
- Reward definition canonical: `66505d4139b1e36cc5f5189fe1f55ac22906afd98f853a330368f38929ff967f`
- Reward catalog canonical: `d797c28ae65153e38d487bf29e7becf99f6afab8c828a3ce6f79d48764be56e0`
- Upstream projection canonical: `9830747edfb3de3318910795820588b2114d28bf108e79fb5b76ef9f6cf664d3`
- Upstream Item catalog canonical: `d0dd84f6e2f59344d3a49416d883eb5f4f84b408ab85a96a8dcf6751de97e357`

## Changed Files

- `Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Data.meta`
- `Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Data/FirstClear.meta`
- `Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Data/FirstClear/C1Stage1FirstClearGuaranteedRewardData.cs`
- `Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Data/FirstClear/C1Stage1FirstClearGuaranteedRewardData.cs.meta`
- `Assets/_Game/Scripts/TalismanBag/Editor/V04/RewardDrop/C1Stage1FirstClearGuaranteedRewardData01Verifier.cs`
- `Assets/_Game/Scripts/TalismanBag/Editor/V04/RewardDrop/C1Stage1FirstClearGuaranteedRewardData01Verifier.cs.meta`
- `Docs/V0.4/Reports/C1Stage1FirstClearGuaranteedRewardData01Report.md`
- `Docs/V0.4/Reports/C1Stage1FirstClearGuaranteedRewardData01Spec.csv`
- `Docs/V0.4/Reports/C1Stage1FirstClearGuaranteedRewardData01CanonicalDeterminism.csv`

## Protected Dependency Hashes

- `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1Lv1StarterItemBaseline.cs`: `a4c454ffcc0a3b5ce1bf2181a39b79ca73c809fd81fd5d3aa3257c7908a140c7`
- `Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Contracts/RewardDropContractPrimitives.cs`: `60385bc84807b88122006948ab402fba39c5d3ec5721d6a387da842f3c7e05e3`
- `Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Contracts/RewardDropContractValidator.cs`: `ed60a14babef252be4d9e646ea9ad16712b770f119d67e7bde2182170522a796`
- `Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Contracts/RewardResult.cs`: `13a6698bd8d3b15ce21222df0edfb66d69e7f3b2bf09f08608af5cd0e8687cce`
- `Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Contracts/RewardClaimAndDedupeIdentities.cs`: `318573d6fc2378b33c7e0bfe8a2357328ead2971d2b9bdc5f9c3466906f2494c`

## Verification

- Focused standalone compilation: `PASS when this executable verifier is running`; runtime source plus its exact pure-C# contract/projection sources are the compilation inputs.
- Dedicated deterministic verifier: `PASS`
- Unity batch / Play / Scene / Prefab serialization: `NOT_RUN`
- User hand test: `NOT_APPLICABLE`

## Known Limitations

- Static data only: no live clear-event binding, reward-result production, generated Item instance, claim ledger, Inventory, Save, UI, Scene, ordinary pool, repeat/offline/Boss data, or APK inclusion claim.
- Future entitlement integration must combine the stable claim scope with an authoritative player/profile identity owned elsewhere.
- Owned compiler/verifier PID cleanup remains the invoking task's bounded process responsibility.

## Assertion Matrix

- `PASS` boundary / assignment-sha-before (`cd0c1492d9319ff258e5f3725182a8c1294b4b8ccc97c33b07ac7a0be2ee76b0`)
- `PASS` positive / catalog-valid (``)
- `PASS` positive / catalog-schema (`C1Stage1FirstClearGuaranteedRewardCatalog.v1`)
- `PASS` positive / catalog-exact-row-count (`1`)
- `PASS` positive / accepted-query-resolves (`NONE`)
- `PASS` positive / definition-schema (`C1Stage1FirstClearGuaranteedRewardDefinition.v1`)
- `PASS` positive / exact-stage-and-context (`CAMPAIGN_NORMAL_LV1/bone_aspect_chapter_1/1-1`)
- `PASS` positive / fixed-policy (`FirstClearOnline/FirstClearFixed/1`)
- `PASS` positive / explicit-ordinary-subject (`I002@white/OrdinaryGeneratedItem/C1Lv1StarterItemFactProjection.v1/1`)
- `PASS` positive / source-and-entitlement-identities (`c1.first_clear.1-1.i002.white.v1/campaign_normal_lv1.bone_aspect_chapter_1.1-1.first_clear_fixed/reward.first_clear.entitlement.v1`)
- `PASS` positive / upstream-projection-query (`NONE`)
- `PASS` positive / upstream-canonicals (`9830747edfb3de3318910795820588b2114d28bf108e79fb5b76ef9f6cf664d3/d0dd84f6e2f59344d3a49416d883eb5f4f84b408ab85a96a8dcf6751de97e357`)
- `PASS` positive / claim-scope-independent-of-clear-fixture (`campaign_normal_lv1.bone_aspect_chapter_1.1-1.first_clear_fixed/campaign_normal_lv1.bone_aspect_chapter_1.1-1.first_clear_fixed`)
- `PASS` negative / null-query (`QUERY_REQUIRED`)
- `PASS` negative / dev-showcase-context (`CONTEXT_MISMATCH`)
- `PASS` negative / diagnostics-only-context (`CONTEXT_MISMATCH`)
- `PASS` negative / excluded-mode-RepeatChallengeOnline (`ACQUISITION_MODE_MISMATCH`)
- `PASS` negative / excluded-mode-OfflinePatrol (`ACQUISITION_MODE_MISMATCH`)
- `PASS` negative / excluded-grant-OrdinaryRoll (`GRANT_KIND_MISMATCH`)
- `PASS` negative / excluded-grant-BossFirstClearGuaranteed (`GRANT_KIND_MISMATCH`)
- `PASS` negative / excluded-grant-SystemGrant (`GRANT_KIND_MISMATCH`)
- `PASS` negative / excluded-grant-StoryGrant (`GRANT_KIND_MISMATCH`)
- `PASS` negative / excluded-grant-CosmeticGrant (`GRANT_KIND_MISMATCH`)
- `PASS` negative / excluded-subject-OrdinaryResource (`SUBJECT_KIND_MISMATCH`)
- `PASS` negative / excluded-subject-SystemRhythmSpecial (`SUBJECT_KIND_MISMATCH`)
- `PASS` negative / excluded-subject-StoryCritical (`SUBJECT_KIND_MISMATCH`)
- `PASS` negative / excluded-subject-CosmeticSkin (`SUBJECT_KIND_MISMATCH`)
- `PASS` negative / excluded-subject-BossExclusive (`SUBJECT_KIND_MISMATCH`)
- `PASS` negative / wrong-chapter (`CHAPTER_MISMATCH`)
- `PASS` negative / wrong-stage (`STAGE_MISMATCH`)
- `PASS` negative / missing-base-identity (`SUBJECT_BASE_ITEM_ID_REQUIRED`)
- `PASS` negative / missing-rarity-version-identity (`SUBJECT_IDENTITY_REQUIRED`)
- `PASS` negative / malformed-item-version (`PROJECTION_LOOKUP_FAILED`)
- `PASS` negative / wrong-profile-revision (`PROFILE_REVISION_MISMATCH`)
- `PASS` negative / wrong-projection-canonical (`PROJECTION_CANONICAL_MISMATCH`)
- `PASS` negative / wrong-item-catalog-canonical (`ITEM_CATALOG_CANONICAL_MISMATCH`)
- `PASS` negative / wrong-source-catalog (`SUBJECT_SOURCE_CATALOG_MISMATCH`)
- `PASS` negative / wrong-classification-version (`SUBJECT_CLASSIFICATION_VERSION_MISMATCH`)
- `PASS` negative / unknown-ordinary-item (`PROJECTION_LOOKUP_FAILED`)
- `PASS` negative / special-category-reason-is-explicit (`SUBJECT_KIND_MISMATCH/SUBJECT_KIND_MISMATCH`)
- `PASS` determinism / canonical-repeat-100 (`byte-identical x100`)
- `PASS` determinism / canonical-reversed-fixture (`byte-identical`)
- `PASS` isolation / runtime-excludes-unityengine (`UnityEngine`)
- `PASS` isolation / runtime-excludes-unityeditor (`UnityEditor`)
- `PASS` isolation / runtime-excludes-monobehaviour (`MonoBehaviour`)
- `PASS` isolation / runtime-excludes-scriptableobject (`ScriptableObject`)
- `PASS` isolation / runtime-excludes-directdamage (`directDamage`)
- `PASS` isolation / runtime-excludes-cooldownseconds (`cooldownSeconds`)
- `PASS` isolation / runtime-excludes-lightingrequirement (`lightingRequirement`)
- `PASS` isolation / runtime-excludes-iteminstanceid (`itemInstanceId`)
- `PASS` isolation / runtime-excludes-instancecanonicalsignature (`instanceCanonicalSignature`)
- `PASS` isolation / runtime-excludes-system-random (`System.Random`)
- `PASS` isolation / runtime-excludes-datetime (`DateTime`)
- `PASS` isolation / runtime-excludes-guid (`Guid.`)
- `PASS` isolation / runtime-excludes-probability (`probability`)
- `PASS` isolation / runtime-excludes-weight (`weight`)
- `PASS` isolation / runtime-excludes-pity (`pity`)
- `PASS` isolation / runtime-excludes-affix (`affix`)
- `PASS` isolation / runtime-excludes-droprequest (`DropRequest`)
- `PASS` isolation / runtime-excludes-droprollresult (`DropRollResult`)
- `PASS` isolation / runtime-excludes-rewardresult (`RewardResult`)
- `PASS` isolation / runtime-excludes-stageclearfact (`StageClearFact`)
- `PASS` isolation / runtime-excludes-inventory (`Inventory`)
- `PASS` isolation / runtime-excludes-save (`Save`)
- `PASS` isolation / runtime-excludes-item-stat-literals (`none`)
- `PASS` isolation / runtime-namespace (`runtime source`)
- `PASS` immutability / public-mutation-surface (`none`)
- `PASS` immutability / catalog-collection-read-only (`True`)
- `PASS` protected-source / protected-hash-C1Lv1StarterItemBaseline (`a4c454ffcc0a3b5ce1bf2181a39b79ca73c809fd81fd5d3aa3257c7908a140c7`)
- `PASS` protected-source / protected-hash-RewardDropContractPrimitives (`60385bc84807b88122006948ab402fba39c5d3ec5721d6a387da842f3c7e05e3`)
- `PASS` protected-source / protected-hash-RewardDropContractValidator (`ed60a14babef252be4d9e646ea9ad16712b770f119d67e7bde2182170522a796`)
- `PASS` protected-source / protected-hash-RewardResult (`13a6698bd8d3b15ce21222df0edfb66d69e7f3b2bf09f08608af5cd0e8687cce`)
- `PASS` protected-source / protected-hash-RewardClaimAndDedupeIdentities (`318573d6fc2378b33c7e0bfe8a2357328ead2971d2b9bdc5f9c3466906f2494c`)
- `PASS` boundary / assignment-sha-after (`cd0c1492d9319ff258e5f3725182a8c1294b4b8ccc97c33b07ac7a0be2ee76b0`)
- `PASS` write-scope / whitelist-file-data-meta (`172`)
- `PASS` write-scope / whitelist-file-firstclear-meta (`172`)
- `PASS` write-scope / whitelist-file-c1stage1firstclearguaranteedrewarddata-cs (`38845`)
- `PASS` write-scope / whitelist-file-c1stage1firstclearguaranteedrewarddata-cs-meta (`60`)
- `PASS` write-scope / whitelist-file-c1stage1firstclearguaranteedrewarddata01verifier-cs (`54206`)
- `PASS` write-scope / whitelist-file-c1stage1firstclearguaranteedrewarddata01verifier-cs-meta (`60`)
- `PASS` write-scope / whitelist-file-c1stage1firstclearguaranteedrewarddata01report-md (`9649`)
- `PASS` write-scope / whitelist-file-c1stage1firstclearguaranteedrewarddata01spec-csv (`9912`)
- `PASS` write-scope / whitelist-file-c1stage1firstclearguaranteedrewarddata01canonicaldeterminism-csv (`811`)
- `PASS` write-scope / data-folder-exact-file-set (`Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Data/FirstClear.meta|Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Data/FirstClear/C1Stage1FirstClearGuaranteedRewardData.cs|Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Data/FirstClear/C1Stage1FirstClearGuaranteedRewardData.cs.meta`)
- `PASS` write-scope / verifier-prefix-exact-file-set (`Assets/_Game/Scripts/TalismanBag/Editor/V04/RewardDrop/C1Stage1FirstClearGuaranteedRewardData01Verifier.cs|Assets/_Game/Scripts/TalismanBag/Editor/V04/RewardDrop/C1Stage1FirstClearGuaranteedRewardData01Verifier.cs.meta`)
- `PASS` write-scope / report-prefix-exact-file-set (`Docs/V0.4/Reports/C1Stage1FirstClearGuaranteedRewardData01CanonicalDeterminism.csv|Docs/V0.4/Reports/C1Stage1FirstClearGuaranteedRewardData01Report.md|Docs/V0.4/Reports/C1Stage1FirstClearGuaranteedRewardData01Spec.csv`)
- `PASS` reports / report-terminal-marker-c1stage1firstclearguaranteedrewarddata01report-md (`marker scan`)
- `PASS` reports / report-terminal-marker-c1stage1firstclearguaranteedrewarddata01spec-csv (`marker scan`)
- `PASS` reports / report-terminal-marker-c1stage1firstclearguaranteedrewarddata01canonicaldeterminism-csv (`marker scan`)
- `PASS` boundary / assignment-sha-after (`cd0c1492d9319ff258e5f3725182a8c1294b4b8ccc97c33b07ac7a0be2ee76b0`)

C1_STAGE1_FIRST_CLEAR_GUARANTEED_REWARD_DATA01_PASS
