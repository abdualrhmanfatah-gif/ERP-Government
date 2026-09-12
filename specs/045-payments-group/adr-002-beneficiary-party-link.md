# ADR-002: Required beneficiary name with optional Party link

**Date**: 2026-09-09  
**Status**: Accepted — explicit stakeholder answer during the PAY-02 frontend specification amendment.  
**Supersedes**: ADR-001 D-1 only. D-2 through D-6 are unaffected.

The stakeholder explicitly selected «اسم إلزامي مع ربط اختياري بطرف مسجل». Disbursement requests and their resulting payment orders carry required `BeneficiaryName` and nullable `BeneficiaryPartyId`. This supports unregistered beneficiaries without forcing creation of a Party, while retaining traceability when a registered Party exists.

The server validates an explicitly selected Party as existing and active, stores its ID, and snapshots its current name. Without a Party ID, a manually entered nonblank name is accepted. The generated order copies both fields from its source; later Party renaming must not rewrite the historical name. An invalid supplied ID must be refused, not silently dropped.

## Consequences

- Replace the name-only target in spec/data-model/contracts; do not reintroduce ambiguous `BeneficiaryId` or supplier-only terminology.
- Existing name-only records remain valid with a null Party link. Never infer links by name matching or fabricate historical identities.
- Plan nullable references and validation using repository migration rules; do not edit applied migrations. This decision changes the target, not the deployed schema in this documentation task.
- Order integration is limited here to preserving beneficiary fields from the source. PAY-01..04 implementation planning and generated-client regeneration follow separately.
- Name-only was the previous ADR-001 tradeoff; it is superseded by the stakeholder's explicit requirement for optional registered-party traceability.
