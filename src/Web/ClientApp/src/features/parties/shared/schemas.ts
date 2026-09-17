import { z } from 'zod';
import { PartyType, type CreatePartyCommand } from './types';

/**
 * Party form schema — single source for defaults, client validation, and
 * payload conversion (AGENTS.md forms rule: Zod schema per feature entity).
 */

export const partyFormSchema = z.object({
  partyType: z.nativeEnum(PartyType),
  nameAr: z.string().trim().min(1, 'الاسم بالعربية مطلوب'),
  nameEn: z.string().trim().max(200, 'الاسم بالإنجليزية طويل جداً').optional(),
  taxNumber: z.string().trim().max(50, 'الرقم الضريبي طويل جداً').optional(),
  nationalId: z.string().trim().max(50, 'الهوية الوطنية طويلة جداً').optional(),
  phone: z.string().trim().max(30, 'رقم الهاتف طويل جداً').optional(),
  email: z
    .union([z.literal(''), z.string().trim().email('البريد الإلكتروني غير صالح')])
    .optional(),
  address: z.string().trim().max(300, 'العنوان طويل جداً').optional(),
  notes: z.string().trim().max(1000, 'الملاحظات طويلة جداً').optional(),
});

export type PartyFormValues = z.infer<typeof partyFormSchema>;

export const EMPTY_PARTY_FORM: PartyFormValues = {
  partyType: PartyType.Supplier,
  nameAr: '',
};

function emptyToUndefined(value?: string): string | undefined {
  const trimmed = value?.trim();
  return trimmed ? trimmed : undefined;
}

export function toCreatePartyCommand(values: PartyFormValues): CreatePartyCommand {
  return {
    partyType: values.partyType,
    nameAr: values.nameAr.trim(),
    nameEn: emptyToUndefined(values.nameEn),
    taxNumber: emptyToUndefined(values.taxNumber),
    nationalId: emptyToUndefined(values.nationalId),
    phone: emptyToUndefined(values.phone),
    email: emptyToUndefined(values.email),
    address: emptyToUndefined(values.address),
    notes: emptyToUndefined(values.notes),
  };
}

export function toPartyFormValues(data: CreatePartyCommand): PartyFormValues {
  return {
    partyType: data.partyType,
    nameAr: data.nameAr ?? '',
    nameEn: data.nameEn ?? '',
    taxNumber: data.taxNumber ?? '',
    nationalId: data.nationalId ?? '',
    phone: data.phone ?? '',
    email: data.email ?? '',
    address: data.address ?? '',
    notes: data.notes ?? '',
  };
}
