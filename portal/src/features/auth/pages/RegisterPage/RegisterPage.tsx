import { RegisterForm } from '../../components/RegisterForm/RegisterForm';
import styles from './RegisterPage.module.scss';

export const RegisterPage = () => {
  return (
    <div className={styles.registerPage}>
      <RegisterForm />
    </div>
  );
};
