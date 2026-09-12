import { useState, type FormEvent } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../../shared/hooks/useAuth';
import { Page } from '@/components/ui';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import { notify } from '@/features/notifications/notify';

const MIN_PASSWORD_LENGTH = 6;

function validateEmail(value: string): boolean {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value);
}

export function RegisterPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [emailTouched, setEmailTouched] = useState(false);
  const [passwordTouched, setPasswordTouched] = useState(false);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const { register } = useAuth();
  const navigate = useNavigate();

  const emailValid = validateEmail(email);
  const passwordValid = password.length >= MIN_PASSWORD_LENGTH;

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    setEmailTouched(true);
    setPasswordTouched(true);
    if (!emailValid || !passwordValid) return;
    setLoading(true);
    try {
      await register(email, password);
      notify({ type: 'success', title: 'تم إنشاء الحساب بنجاح' });
      navigate('/login');
    } catch {
      setError('فشل إنشاء الحساب. يرجى المحاولة مرة أخرى.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Page title="إنشاء حساب جديد" maxWidth="sm">
      {error ? (
        <div role="alert" className="p-3 bg-[color-mix(in_srgb,var(--color-error)_8%,transparent)] rounded-lg text-sm text-[var(--color-error)] mb-4 text-center">
          {error}
        </div>
      ) : null}
      <form onSubmit={handleSubmit} aria-label="إنشاء حساب">
        <Input
          label="البريد الإلكتروني"
          type="email"
          autoComplete="username"
          value={email}
          onChange={(e) => { setEmail(e.target.value); setEmailTouched(true); setError(''); }}
          onBlur={() => setEmailTouched(true)}
          error={emailTouched && !emailValid ? 'البريد الإلكتروني غير صالح' : undefined}
          required
          disabled={loading}
        />
        <Input
          label="كلمة المرور"
          type="password"
          autoComplete="new-password"
          value={password}
          onChange={(e) => { setPassword(e.target.value); setPasswordTouched(true); setError(''); }}
          onBlur={() => setPasswordTouched(true)}
          error={passwordTouched && !passwordValid ? `كلمة المرور يجب أن تكون ${MIN_PASSWORD_LENGTH} أحرف على الأقل` : undefined}
          required
          disabled={loading}
          className="mt-3"
        />
        <Button
          type="submit"
          variant="primary"
          loading={loading}
          disabled={loading}
          className="w-full mt-4"
        >
          إنشاء حساب
        </Button>
        <p className="mt-4 text-center text-sm">
          لديك حساب بالفعل؟{' '}
          <Link to="/login" className="text-[var(--color-primary)]">تسجيل الدخول</Link>
        </p>
      </form>
    </Page>
  );
}
