import { useSearchParams } from 'react-router-dom';
import { PlanFeed } from '@/features/Plan/components/PlanFeed/PlanFeed';
import { PlanSelection } from '@/features/Plan/components/PlanSelection/PlanSelection';

export const PlanPage = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const type = searchParams.get('type');

  const isValidType = type === 'course' || type === 'ecommerce';

  if (isValidType) {
    return <PlanFeed category={type as 'course' | 'ecommerce'} />;
  }

  return (
    <PlanSelection
      onSelect={(category) => {
        setSearchParams({ type: category });
      }}
    />
  );
};
