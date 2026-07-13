import { authService } from '@/features/auth/services/auth.service';
import { LoginForm } from '@/features/auth/components/LoginForm/LoginForm';
import styles from './LoginPage.module.scss';
import logoAdmin from '@/assets/logo-admin.png';


export const LoginPage = () => {
  return (
    <div className={styles.loginPage}>
      <div className={styles.loginCard}>
        <div className={styles.brandHeader}>
          <img src={logoAdmin} alt="Logo Admin" className={styles.logo} />
          <p className={styles.subtitle}>Bem-vindo ao Painel Admin</p>
        </div>

        <LoginForm />
      </div>
    </div>
  );
};
