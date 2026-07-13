import { LoginForm } from '../../components/LoginForm/LoginForm';
import styles from './LoginPage.module.scss';

export const LoginPage = () => {
  return (
    <div className={styles.loginPage}>
      <LoginForm />
    </div>
  );
};
