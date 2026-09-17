/**
 * Component Gallery — Internal development reference
 *
 * The single visual reference for the Phase 1 core components: approved
 * variants, important interaction states, semantic roles, status vocabulary
 * and both densities. Gated to dev-only via the router (`/__gallery__`);
 * never available in production builds.
 *
 * Contracts: specs/052-unified-ui-language/contracts/
 */
import { useState } from 'react';
import { StatusBadge, type BadgeVariant } from '../StatusBadge';
import { Badge } from '../Badge';
import { Button } from '../Button';
import { ButtonBar } from '../ButtonBar';
import { Card } from '../Card';
import { Alert } from '../Alert';
import { EmptyState } from '../EmptyState';
import { ErrorState } from '../ErrorState';
import { Loading, Skeleton } from '../Loading';
import { FilterBar } from '../FilterBar';
import { FilterSearch } from '../FilterSearch';
import { FilterSelect } from '../FilterSelect';
import { FilterDate } from '../FilterDate';
import { DataGrid, type DataGridColumn } from '../DataGrid';
import { MobileCard, MobileCardField } from '../MobileCard';
import { Pagination } from '../Pagination';
import { MoneyDisplay } from '../MoneyDisplay';
import { Dialog } from '../Dialog';
import { ConfirmDialog } from '../ConfirmDialog';
import { Sheet } from '../Sheet';
import { Input } from '../Input';
import { Textarea } from '../Textarea';
import { Select } from '../Select';
import { Combobox } from '../Combobox';
import { Switch } from '../Switch';
import { DatePicker } from '../DatePicker';
import { FormField } from '../FormField';
import { Label } from '../Label';
import { Breadcrumb } from '../Breadcrumb';
import { Tabs } from '../Tabs';
import { Page } from '../Page';
import { notify } from '../Toast';

const statusVariants: { variant: BadgeVariant; label: string }[] = [
  { variant: 'draft', label: 'مسودة' },
  { variant: 'pending', label: 'قيد المراجعة' },
  { variant: 'approved', label: 'موافق' },
  { variant: 'active', label: 'نشط' },
  { variant: 'closed', label: 'مغلق' },
  { variant: 'inactive', label: 'غير نشط' },
  { variant: 'posted', label: 'مرحل' },
  { variant: 'reversed', label: 'معكوس' },
  { variant: 'cancelled', label: 'ملغى' },
  { variant: 'locked', label: 'مقفل' },
  { variant: 'submitted', label: 'مرسلة' },
  { variant: 'sentToTreasury', label: 'مرسلة للخزينة' },
  { variant: 'paid', label: 'مدفوعة' },
  { variant: 'disbursed', label: 'صرفت' },
  { variant: 'partiallyPaid', label: 'مدفوعة جزئياً' },
  { variant: 'passed', label: 'ناجح' },
  { variant: 'rejected', label: 'مرفوضة' },
  { variant: 'failed', label: 'فاشلة' },
  { variant: 'voided', label: 'ملغاة نهائياً' },
  { variant: 'warning', label: 'تحذير' },
  { variant: 'overBudget', label: 'يتجاوز الميزانية' },
  { variant: 'overridden', label: 'تم التجاوز' },
  { variant: 'unbalanced', label: 'غير متوازن' },
];

const badgeVariants = ['default', 'primary', 'secondary', 'success', 'warning', 'danger', 'error', 'outline'] as const;
const buttonVariants = ['primary', 'outline', 'secondary', 'ghost', 'destructive', 'success', 'info', 'link'] as const;
const buttonSizes = ['default', 'xs', 'sm', 'lg'] as const;

interface GalleryRow {
  id: number;
  code: string;
  name: string;
  amount: number;
  status: 'active' | 'inactive';
}

const galleryRows: GalleryRow[] = [
  { id: 1, code: 'PTY-0001', name: 'مورد تجريبي', amount: 125000.5, status: 'active' },
  { id: 2, code: 'PTY-0002', name: 'عميل تجريبي', amount: -4200, status: 'inactive' },
];

const galleryColumns: DataGridColumn<GalleryRow>[] = [
  { header: 'الكود', cell: (row) => <span className="font-mono" dir="ltr">{row.code}</span> },
  { header: 'الاسم', cell: (row) => row.name },
  { header: 'المبلغ', align: 'right', cell: (row) => <MoneyDisplay value={row.amount} /> },
  { header: 'الحالة', cell: (row) => <StatusBadge variant={row.status}>{row.status === 'active' ? 'نشط' : 'غير نشط'}</StatusBadge> },
];

function Section({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <section className="space-y-3">
      <h2 className="text-headline-sm font-bold text-[var(--color-on-surface)] border-b border-[var(--color-outline-variant)] pb-2">
        {title}
      </h2>
      {children}
    </section>
  );
}

function Row({ label, children }: { label?: string; children: React.ReactNode }) {
  return (
    <div className="flex flex-wrap items-center gap-3">
      {label && <span className="text-xs text-[var(--color-on-surface-variant)] w-24 shrink-0">{label}</span>}
      {children}
    </div>
  );
}

function Swatch({ name, value }: { name: string; value: string }) {
  return (
    <div className="flex items-center gap-2">
      <span
        className="inline-block size-6 rounded border border-[var(--color-outline-variant)]"
        style={{ background: value }}
        aria-hidden="true"
      />
      <span className="text-xs text-[var(--color-on-surface-variant)] font-mono" dir="ltr">{name}</span>
    </div>
  );
}

export function ComponentGallery() {
  const [dialogOpen, setDialogOpen] = useState(false);
  const [confirmOpen, setConfirmOpen] = useState(false);
  const [sheetOpen, setSheetOpen] = useState(false);
  const [filterSearch, setFilterSearch] = useState('');
  const [filterSelect, setFilterSelect] = useState('');
  const [filterDate, setFilterDate] = useState('');
  const [page, setPage] = useState(1);
  const [comboboxValue, setComboboxValue] = useState('');
  const [switchChecked, setSwitchChecked] = useState(true);
  const [dateValue, setDateValue] = useState('');
  const [selectValue, setSelectValue] = useState('');

  return (
    <div className="flex flex-col gap-8 p-6 min-h-[calc(100vh-64px)] max-w-5xl">
      <header>
        <h1 className="text-headline-md font-bold text-[var(--color-on-surface)]">مكتبة المكونات</h1>
        <p className="text-sm text-[var(--color-on-surface-variant)] mt-1">
          المرجع البصري الموحد للمكونات الأساسية — النسخ والحالات والأدوار وأنظمة الكثافة. غير متاح في بيئة الإنتاج.
        </p>
      </header>

      {/* ── Status roles + aliases ──────────────────────────────────────── */}
      <Section title="أدوار الحالة — القاموس المعتمد (status-semantics.md)">
        <Row label="الأدوار الستة">
          {statusVariants.slice(0, 6).map(({ variant, label }) => (
            <StatusBadge key={variant} variant={variant}>{label}</StatusBadge>
          ))}
        </Row>
        <Row label="الأسماء البديلة sm">
          {statusVariants.slice(6).map(({ variant, label }) => (
            <StatusBadge key={variant} variant={variant} size="sm">{label}</StatusBadge>
          ))}
        </Row>
      </Section>

      {/* ── Badge (metadata only) ───────────────────────────────────────── */}
      <Section title="Badge — بيانات وصفية فقط (ليست حالة دورة حياة)">
        <Row>
          {badgeVariants.map((v) => (
            <Badge key={v} variant={v}>{v}</Badge>
          ))}
          <Badge variant="default">5 نتائج</Badge>
        </Row>
      </Section>

      {/* ── Semantic roles ──────────────────────────────────────────────── */}
      <Section title="الأدوار الدلالية — الأسطح والنصوص والحدود والحالات">
        <Row label="الأسطح">
          <Swatch name="surface" value="var(--color-surface)" />
          <Swatch name="surface-container-lowest" value="var(--color-surface-container-lowest)" />
          <Swatch name="surface-container-low" value="var(--color-surface-container-low)" />
          <Swatch name="surface-container" value="var(--color-surface-container)" />
          <Swatch name="surface-container-high" value="var(--color-surface-container-high)" />
        </Row>
        <Row label="النصوص">
          <span className="text-[var(--color-on-surface)] text-sm">نص أساسي</span>
          <span className="text-[var(--color-on-surface-variant)] text-sm">نص ثانوي</span>
          <span className="text-[var(--color-disabled-fg)] text-sm">نص معطل</span>
          <span className="text-[var(--color-link)] text-sm">رابط</span>
        </Row>
        <Row label="الحدود">
          <Swatch name="outline" value="var(--color-outline)" />
          <Swatch name="outline-variant" value="var(--color-outline-variant)" />
          <Swatch name="input-border" value="var(--color-input-border)" />
          <Swatch name="container-border" value="var(--color-container-border)" />
        </Row>
        <Row label="الحالات">
          <Swatch name="success" value="var(--color-success)" />
          <Swatch name="warning" value="var(--color-warning)" />
          <Swatch name="error" value="var(--color-error)" />
          <Swatch name="info" value="var(--color-info)" />
          <Swatch name="disabled-bg" value="var(--color-disabled-bg)" />
        </Row>
      </Section>

      {/* ── Densities ───────────────────────────────────────────────────── */}
      <Section title="أنظمة الكثافة — compact / comfortable">
        <Row label="compact (قوائم)">
          <div className="flex items-center gap-3">
            <div className="h-[var(--density-compact-control-height)] w-48 rounded-lg border border-[var(--color-input-border)] bg-[var(--color-surface-container-lowest)] px-3 flex items-center text-sm text-[var(--color-on-surface)]">
              ارتفاع 36px
            </div>
            <span className="text-xs text-[var(--color-on-surface-variant)]">
              زوج الحقول 8px · تباعد الأقسام 16px · حشوة الخلية 8px
            </span>
          </div>
        </Row>
        <Row label="comfortable (نماذج)">
          <div className="flex items-center gap-3">
            <div className="h-[var(--density-comfortable-control-height)] w-48 rounded-lg border border-[var(--color-input-border)] bg-[var(--color-surface-container-lowest)] px-3 flex items-center text-sm text-[var(--color-on-surface)]">
              ارتفاع 44px
            </div>
            <span className="text-xs text-[var(--color-on-surface-variant)]">
              زوج الحقول 16px · تباعد الأقسام 24px · حشوة الخلية 12px
            </span>
          </div>
        </Row>
      </Section>

      {/* ── Breadcrumb / Tabs ───────────────────────────────────────────── */}
      <Section title="Breadcrumb / Tabs">
        <Breadcrumb
          items={[
            { label: 'الرئيسية', path: '/dashboard' },
            { label: 'الأطراف', path: '/parties' },
            { label: 'طرف' },
          ]}
        />
        <Tabs
          tabs={[
            { key: 'summary', label: 'ملخص', content: <p className="text-sm">محتوى الملخص</p> },
            { key: 'details', label: 'تفاصيل', content: <p className="text-sm">محتوى التفاصيل</p> },
          ]}
        />
      </Section>

      {/* ── Page skeleton ───────────────────────────────────────────────── */}
      <Section title="Page — تركيب الصفحة">
        <div className="overflow-hidden rounded-xl border border-[var(--color-container-border)]">
          <Page
            className="min-h-0"
            title="عنوان الصفحة"
            description="وصف يظهر فقط عندما يضيف سياقاً لا يوفره العنوان"
            breadcrumbs={[{ label: 'الرئيسية', path: '/' }, { label: 'القسم' }]}
            actions={<Button variant="primary" size="sm">إجراء رئيسي</Button>}
            toolbar={<FilterBar hasFilters={false}><FilterSearch value="" onChange={() => {}} /></FilterBar>}
          >
            <p className="text-sm text-[var(--color-on-surface-variant)]">محتوى الصفحة</p>
          </Page>
        </div>
      </Section>

      {/* ── Button / ButtonBar ──────────────────────────────────────────── */}
      <Section title="Button — الألوان والأحجام والحالات">
        {buttonSizes.map((size) => (
          <Row key={size} label={size}>
            {buttonVariants.map((variant) => (
              <Button key={variant} variant={variant} size={size}>
                {variant}
              </Button>
            ))}
          </Row>
        ))}
        <Row label="حالة التحميل">
          <Button variant="primary" size="sm" loading>جارٍ الحفظ</Button>
          <Button variant="outline" size="sm" disabled>معطل</Button>
        </Row>
        <Row label="ButtonBar (افتراضي=outline)">
          <ButtonBar
            actions={[
              { key: 'save', label: 'حفظ', variant: 'primary', onClick: () => {} },
              { key: 'print', label: 'طباعة', onClick: () => {} },
              { key: 'delete', label: 'حذف', variant: 'destructive', onClick: () => {} },
              { key: 'disabled', label: 'غير متاح', disabled: true, disabledReason: 'لا توجد صلاحية', onClick: () => {} },
            ]}
          />
        </Row>
      </Section>

      {/* ── Card / Alert ────────────────────────────────────────────────── */}
      <Section title="Card — الأنواع / Alert — الحالات">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <Card variant="default">
            <h3 className="text-label-md font-semibold mb-2">افتراضي</h3>
            <p className="text-sm text-[var(--color-on-surface-variant)]">سطح افتراضي للنموذج أو الملخص</p>
          </Card>
          <Card variant="flat">
            <h3 className="text-label-md font-semibold mb-2">مسطّح</h3>
            <p className="text-sm text-[var(--color-on-surface-variant)]">دون حدود</p>
          </Card>
          <Card variant="outlined">
            <h3 className="text-label-md font-semibold mb-2">محاط</h3>
            <p className="text-sm text-[var(--color-on-surface-variant)]">خلفية شفافة</p>
          </Card>
        </div>
        <Alert variant="info">معلومات — هذا تنبيه توضيحي</Alert>
        <Alert variant="success">نجاح — تم الحفظ بنجاح</Alert>
        <Alert variant="warning">تحذير — يرجى المراجعة</Alert>
        <Alert variant="error" role="alert">خطأ — فشلت العملية</Alert>
        <Row>
          <Button variant="outline" size="sm" onClick={() => notify({ type: 'info', title: 'إشعار معلوماتي', message: 'رسالة توضيحية داخل الإشعار' })}>
            إظهار إشعار Toast
          </Button>
        </Row>
      </Section>

      {/* ── Form controls ───────────────────────────────────────────────── */}
      <Section title="حقول النموذج — النمط المعتمد (التحكم يملك التسمية والخطأ)">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <Input label="نص عادي" placeholder="اكتب هنا" />
          <Input label="حقل مطلوب" required error="هذا الحقل مطلوب" />
          <Input label="حقل معطل" disabled value="قيمة" readOnly />
          <Select
            label="قائمة"
            required
            value={selectValue}
            onChange={(e) => setSelectValue(e.target.value)}
            options={[
              { value: '', label: 'اختر...' },
              { value: 'a', label: 'الخيار الأول' },
              { value: 'b', label: 'الخيار الثاني' },
            ]}
          />
          <Combobox
            label="قائمة بحث"
            value={comboboxValue}
            onChange={setComboboxValue}
            options={[
              { value: 'a', label: 'الخيار الأول' },
              { value: 'b', label: 'الخيار الثاني' },
            ]}
          />
          <DatePicker label="تاريخ" value={dateValue} onChange={setDateValue} required />
          <Textarea label="ملاحظات" rows={3} error="رسالة خطأ توضيحية" />
          <div className="flex flex-col gap-2">
            <Label required>تسمية مستقلة</Label>
            <Switch checked={switchChecked} onChange={setSwitchChecked} label="مفتاح تشغيل" />
            <FormField label="حقل مركّب (FormField)" htmlFor="gallery-custom-control" description="يستخدم للحقول المخصصة فقط">
              <input
                id="gallery-custom-control"
                className="h-[var(--density-comfortable-control-height)] w-full rounded-lg border-2 border-[var(--color-input-border)] bg-[var(--color-surface-container-lowest)] px-3 text-sm"
                placeholder="تحكم مخصص"
              />
            </FormField>
          </div>
        </div>
      </Section>

      {/* ── Filters ─────────────────────────────────────────────────────── */}
      <Section title="FilterBar — شريط الفلاتر (كثافة compact)">
        <FilterBar
          hasFilters={!!filterSearch || !!filterSelect || !!filterDate}
          onClear={() => { setFilterSearch(''); setFilterSelect(''); setFilterDate(''); }}
        >
          <FilterSearch value={filterSearch} onChange={setFilterSearch} placeholder="بحث..." />
          <FilterSelect
            label="الحالة"
            value={filterSelect}
            onChange={setFilterSelect}
            options={[
              { value: '', label: 'الكل' },
              { value: 'active', label: 'نشط' },
              { value: 'inactive', label: 'غير نشط' },
            ]}
          />
          <FilterDate label="التاريخ" value={filterDate} onChange={setFilterDate} />
        </FilterBar>
      </Section>

      {/* ── Data presentation ───────────────────────────────────────────── */}
      <Section title="DataGrid / MobileCard / Pagination / MoneyDisplay">
        <DataGrid
          columns={galleryColumns}
          data={galleryRows}
          rowKey={(row) => row.id}
          emptyMessage="لا توجد بيانات"
        />
        <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
          <MobileCard>
            <div className="grid grid-cols-2 gap-3">
              <MobileCardField label="الاسم" value="مورد تجريبي" />
              <MobileCardField label="المبلغ" value={<MoneyDisplay value={125000.5} currency="YER" />} />
            </div>
          </MobileCard>
          <MobileCard>
            <div className="grid grid-cols-2 gap-3">
              <MobileCardField label="الاسم" value="عميل تجريبي" />
              <MobileCardField label="المبلغ" value={<MoneyDisplay value={-4200} />} />
            </div>
          </MobileCard>
        </div>
        <Row label="المبالغ">
          <MoneyDisplay value={1234567.89} />
          <MoneyDisplay value={-250} currency="USD" />
        </Row>
        <Pagination page={page} pageSize={10} total={42} onChange={setPage} />
      </Section>

      {/* ── Feedback states ─────────────────────────────────────────────── */}
      <Section title="EmptyState / ErrorState / Loading / Skeleton">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <EmptyState message="لا توجد أطراف بعد" />
          <EmptyState message="لا توجد نتائج مطابقة لمعايير البحث" />
          <ErrorState message="حدث خطأ أثناء تحميل البيانات" onRetry={() => {}} />
          <div className="flex flex-col items-center justify-center gap-4">
            <Loading />
            <Skeleton variant="text" lines={2} />
            <Skeleton variant="table" lines={2} />
          </div>
        </div>
      </Section>

      {/* ── Overlays ────────────────────────────────────────────────────── */}
      <Section title="Dialog / ConfirmDialog / Sheet">
        <Row>
          <Button variant="outline" size="sm" onClick={() => setDialogOpen(true)}>فتح نافذة</Button>
          <Button variant="destructive" size="sm" onClick={() => setConfirmOpen(true)}>حذف (تأكيد)</Button>
          <Button variant="outline" size="sm" onClick={() => setSheetOpen(true)}>فتح لوحة جانبية</Button>
        </Row>
        <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} title="عنوان النافذة">
          <p className="text-sm text-[var(--color-on-surface)]">محتوى النافذة المنبثقة هنا. جرّب الإغلاق بمفتاح Escape.</p>
        </Dialog>
        <ConfirmDialog
          open={confirmOpen}
          onClose={() => setConfirmOpen(false)}
          onConfirm={() => setConfirmOpen(false)}
          title="تأكيد الحذف"
          message="هل أنت متأكد من حذف هذا العنصر؟ لا يمكن التراجع عن هذا الإجراء."
          confirmLabel="حذف"
          destructive
        />
        <Sheet open={sheetOpen} onClose={() => setSheetOpen(false)} title="لوحة جانبية">
          <p className="text-sm text-[var(--color-on-surface)]">محتوى اللوحة الجانبية.</p>
        </Sheet>
      </Section>
    </div>
  );
}
