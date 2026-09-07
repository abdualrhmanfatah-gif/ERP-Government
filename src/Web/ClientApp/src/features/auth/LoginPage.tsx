import { useState, type FormEvent } from 'react';
import { useNavigate, useLocation, Link } from 'react-router-dom';
import { useAuth } from '../../shared/hooks/useAuth';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import { Card } from '@/components/ui';
import { showToast } from '@/components/ui/Toast';

export function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await login(email, password);
      const returnUrl = (location.state as { returnUrl?: string })?.returnUrl ?? '/';
      showToast('success', 'تم تسجيل الدخول بنجاح');
      navigate(returnUrl, { replace: true });
    } catch {
      setError('بريد إلكتروني أو كلمة مرور غير صحيحة');
    } finally {
      setLoading(false);
    }
  };

  return (
    <article className="w-full max-w-md">
      <div className="bg-[var(--color-surface-container-low)] rounded-2xl border border-[var(--color-border-container)] shadow-lg p-8">
        <div className="text-center mb-8">
          <h1 className="text-headline-lg font-bold text-[var(--color-on-surface)]">تسجيل الدخول</h1>
          <p className="mt-2 text-sm text-[var(--color-on-surface-variant)]">أدخل بياناتك للوصول إلى النظام</p>
        </div>

        {error ? (
          <div role="alert" className="p-3 bg-[var(--color-error-container)] rounded-lg text-sm text-[var(--color-on-error-container)] mb-6 text-center border border-[color-mix(in_srgb,var(--color-error)_20%,transparent)]">
            {error}
          </div>
        ) : null}

        <form onSubmit={handleSubmit} className="space-y-5">
          <Input
            label="البريد الإلكتروني"
            type="email"
            autoComplete="username"
            value={email}
            onChange={(e) => { setEmail(e.target.value); setError(''); }}
            required
            disabled={loading}
          />
          <Input
            label="كلمة المرور"
            type="password"
            autoComplete="current-password"
            value={password}
            onChange={(e) => { setPassword(e.target.value); setError(''); }}
            required
            disabled={loading}
          />
          <Button
            type="submit"
            variant="primary"
            loading={loading}
            disabled={loading}
            className="w-full h-11 text-base"
          >
            تسجيل الدخول
          </Button>
        </form>

        <div className="mt-6 pt-5 border-t border-[var(--color-border-container)] text-center">
          <p className="text-sm text-[var(--color-on-surface-variant)]">
            ليس لديك حساب؟{' '}
            <Link to="/register" className="text-[var(--color-primary)] font-medium hover:underline">
              إنشاء حساب
            </Link>
          </p>
        </div>
      </div>
    </article>
  );
}
