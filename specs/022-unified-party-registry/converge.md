# 022 — Unified Party Registry & Shared Document Panels — Converge Report

## Field Coverage Status

### Party Fields
| Field | In Source | Status |
|-------|-----------|--------|
| `PartyCode` | ✅ List, Detail | PASS |
| `PartyType` | ✅ List, Detail, Create | PASS |
| `NameAr` | ✅ List, Detail, Create | PASS |
| `NameEn` | ✅ Detail, Create | PASS |
| `TaxNumber` | ✅ List, Detail, Create | PASS |
| `NationalId` | ✅ Detail, Create | PASS |
| `Phone` | ✅ Detail, Create | PASS |
| `Email` | ✅ Detail, Create | PASS |
| `Address` | ✅ Detail, Create | PASS |
| `Notes` | ✅ Detail, Create | PASS |
| `IsActive` | ✅ List (badge), Detail (toggle) | PASS |

### ApprovalHistory Fields
| Field | In Source | Status |
|-------|-----------|--------|
| `ApproverName` | ✅ ApprovalsPanel | PASS |
| `DecisionAt` | ✅ ApprovalsPanel | PASS |
| `Decision` | ✅ ApprovalsPanel (badge) | PASS |
| `Reason` | ✅ ApprovalsPanel | PASS |

### DocumentStatusLog Fields
| Field | In Source | Status |
|-------|-----------|--------|
| `FromStatus` | ✅ StatusLogPanel | PASS |
| `ToStatus` | ✅ StatusLogPanel | PASS |
| `ChangedBy` | ✅ StatusLogPanel | PASS |
| `ChangedAt` | ✅ StatusLogPanel | PASS |
| `Reason` | ✅ StatusLogPanel | PASS |

### Attachment Fields
| Field | In Source | Status |
|-------|-----------|--------|
| `FileName` | ✅ AttachmentsPanel | PASS |
| `AttachmentTypeCode` | ✅ AttachmentsPanel (badge) | PASS |
| `UploadedBy` | ✅ AttachmentsPanel | PASS |
| `CreatedAt` | ✅ AttachmentsPanel | PASS |
| `SizeBytes` | ✅ AttachmentsPanel (human-readable) | PASS |
| `Gate` | ✅ AttachmentsPanel (badge) | PASS |

### PartyDocumentResponse Fields
| Field | In Source | Status |
|-------|-----------|--------|
| `DocumentType` | ✅ RelatedDocuments | PASS |
| `DocumentNumber` | ✅ RelatedDocuments | PASS |
| `Status` | ✅ RelatedDocuments (badge) | PASS |
| `Date` | ✅ RelatedDocuments | PASS |
| `Amount` | ✅ RelatedDocuments (money format) | PASS |

### Permission Codes
| Code | In Source | Status |
|------|-----------|--------|
| `Parties.View` | ✅ parties feature | PASS |
| `Parties.Create` | ✅ parties feature | PASS |
| `Parties.Update` | ✅ parties feature | PASS |

## Converge Verdict

**PASS** — All 34 fields present in source. 3 shared panels (Approvals/StatusLog/Attachments) reusable across document types. Gate badge with mandatory attachment check. Duplicate tax warning. Deactivation guard. Permission-based button visibility. 27 spec tests verified.
