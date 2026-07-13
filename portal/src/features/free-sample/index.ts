/**
 * Barrel exports para Free Sample Feature (Portal)
 */

// Types
export type { DemoProductItem, ProcessStatus } from './types/free-sample.types';

// Services
export { FreeSampleService } from './services/free-sample.service';

// Hooks
export { useFreeSample } from './hooks/useFreeSample';

// Components
export { ComparisonPanel } from './components/ComparisonPanel/ComparisonPanel';
export { ConversionCTA } from './components/ConversionCTA/ConversionCTA';
export { ProcessingProgress } from './components/ProcessingProgress/ProcessingProgress';
export { UrlInputForm } from './components/UrlInputForm/UrlInputForm';

// Pages
export { FreeSamplePage } from './pages/FreeSamplePage';
