# Componentes Genéricos - Guia de Uso

Todos os componentes agora usam **CSS Modules** para evitar conflitos de estilos e seguem as melhores práticas do projeto.

---

## 📋 Table Component

### Uso Básico
```tsx
import { Table } from '@/components/Table/Table';
import { ActionMenu } from '@/components/ActionMenu/ActionMenu';

const columns = [
  { header: 'Nome', accessor: 'name' as const },
  { header: 'Email', accessor: 'email' as const },
  { 
    header: 'Ações', 
    render: (user) => (
      <ActionMenu 
        onEdit={() => handleEdit(user)}
        onDelete={() => handleDelete(user)}
      />
    ),
    align: 'center' as const,
    width: '100px'
  }
];

<Table
  data={users}
  columns={columns}
  keyExtractor={(user) => user.id}
  onRowClick={(user) => console.log('Clicked:', user)}
  isLoading={loading}
/>
```

### Recursos Avançados
- ✅ Renderização customizada com `render` prop
- ✅ Callback `onRowClick` para linhas clicáveis
- ✅ Classes CSS customizadas por coluna ou linha
- ✅ Alinhamento de células (`left`, `center`, `right`)
- ✅ Mobile-first (transforma em cards no mobile)

---

## 🎴 Card Component

### Composition Pattern
```tsx
import { Card } from '@/components/Card/Card';

<Card data={course} onClick={handleCourseClick}>
  <Card.Image src={course.thumbnail} alt={course.name} badge="Novo" />
  <Card.Body title={course.name}>
    <p>{course.description}</p>
  </Card.Body>
  <Card.Actions>
    <button>Ver Detalhes</button>
    <button>Inscrever-se</button>
  </Card.Actions>
</Card>
```

---

## 📝 Form Components

### ⭐ Composition Pattern (Recomendado)
```tsx
import { Form } from '@/components/Form';

interface LoginForm {
  email: string;
  password: string;
  remember: boolean;
}

<Form<LoginForm> onSubmit={handleLogin} defaultValues={{ remember: false }}>
  <Form.Input 
    name="email" 
    label="Email" 
    type="email" 
    validation={{ required: "Email é obrigatório" }}
    colSpan={12}
  />
  
  <Form.Input 
    name="password" 
    label="Senha" 
    type="password" 
    colSpan={12}
  />
  
  <Form.Checkbox name="remember" label="Lembrar de mim" />
  
  <Form.Actions>
    <Form.Submit isLoading={loading}>Entrar</Form.Submit>
  </Form.Actions>
</Form>
```

### Legacy Config-based (Para compatibilidade)
```tsx
import { GenericForm } from '@/components/Form';

<GenericForm
  fields={[
    { name: 'email', label: 'Email', type: 'email', colSpan: 12 },
    { name: 'password', label: 'Senha', type: 'password', colSpan: 12 }
  ]}
  onSubmit={handleLogin}
  submitText="Entrar"
/>
```

---

## 🎠 Carousel Component

```tsx
import { Carousel } from '@/components/Carousel/Carousel';
import { Card } from '@/components/Card/Card';

<Carousel
  items={courses}
  keyExtractor={(course) => course.id}
  renderItem={(course) => (
    <Card data={course}>
      <Card.Image src={course.thumbnail} alt={course.name} />
      <Card.Body title={course.name}>
        <p>{course.price}</p>
      </Card.Body>
    </Card>
  )}
  options={{ autoplay: { delay: 3000 } }}
/>
```

---

## 📱 Sidebar Component

### Estrutura Simplificada
Agora em `components/SideBar/Sidebar.tsx` (sem aninhamento excessivo)

```tsx
import { Sidebar } from '@/components/SideBar';

const menuItems = [
  { id: 'dashboard', label: 'Dashboard', icon: 'fas fa-home', path: '/' },
  { id: 'courses', label: 'Cursos', icon: 'fas fa-book', path: '/courses' }
];

<Sidebar
  items={menuItems}
  activeItemId="dashboard"
  onItemClick={(item) => navigate(item.path)}
  logo={<img src="/logo.png" alt="Logo" />}
>
  <button onClick={handleLogout}>Sair</button>
</Sidebar>
```

---

## ⚙️ ActionMenu Component

```tsx
import { ActionMenu } from '@/components/ActionMenu/ActionMenu';

// Uso básico
<ActionMenu onEdit={handleEdit} onDelete={handleDelete} />

// Com botões customizados
<ActionMenu onEdit={handleEdit}>
  <button className={styles.item} onClick={handleRefund}>
    <span className="icon">💰</span>
    Estornar
  </button>
</ActionMenu>
```

---

## 🎨 CSS Modules

Todos os componentes agora usam `*.module.scss`:
- ✅ `ActionMenu.module.scss`
- ✅ `Card.module.scss`
- ✅ `Table.module.scss`
- ✅ `Carousel.module.scss`
- ✅ `GenericForm.module.scss`
- ✅ `Sidebar.module.scss`

Isso previne vazamento de estilos globais e garante encapsulamento.

---

## 📦 Imports Recomendados

```tsx
// Components
import { Table } from '@/components/Table/Table';
import { Card } from '@/components/Card/Card';
import { Form } from '@/components/Form'; // Composition Pattern
import { Carousel } from '@/components/Carousel/Carousel';
import { Sidebar } from '@/components/SideBar';
import { ActionMenu } from '@/components/ActionMenu/ActionMenu';
```

---

## 🚀 Melhorias Implementadas

### 1. **CSS Modules Padronizado**
- Classes com escopo local
- Sem conflitos de nomes
- Melhor manutenibilidade

### 2. **Sidebar Reestruturada**
- De `SideBar/components/Sidebar.tsx` → `SideBar/Sidebar.tsx`
- Imports mais limpos
- Melhor organização

### 3. **Form com Composition Pattern**
- Componentes menores e reutilizáveis
- `<Form.Input>`, `<Form.Select>`, etc.
- Melhor DX (Developer Experience)
- Mantém compatibilidade com `GenericForm` legacy

### 4. **Table Super Flexível**
- Renderização customizada por coluna
- Callbacks para cliques em linhas
- Alinhamento e largura de colunas
- Classes CSS customizadas
- Mobile-first com cards automáticos

---

## 📚 Exemplo Completo: CRUD de Usuários

```tsx
import { useState } from 'react';
import { Table, Form, ActionMenu } from '@/components';

function UsersPage() {
  const [users, setUsers] = useState([]);
  const [isFormOpen, setIsFormOpen] = useState(false);

  const columns = [
    { header: 'Nome', accessor: 'name' as const, width: '30%' },
    { header: 'Email', accessor: 'email' as const },
    { 
      header: 'Status', 
      render: (user) => (
        <span className={user.active ? 'badge-success' : 'badge-danger'}>
          {user.active ? 'Ativo' : 'Inativo'}
        </span>
      ),
      align: 'center' as const
    },
    { 
      header: 'Ações', 
      render: (user) => (
        <ActionMenu 
          onEdit={() => handleEdit(user)}
          onDelete={() => handleDelete(user)}
        />
      ),
      align: 'center' as const,
      width: '100px'
    }
  ];

  return (
    <div>
      <Table
        data={users}
        columns={columns}
        keyExtractor={(u) => u.id}
      />

      {isFormOpen && (
        <Form onSubmit={handleSubmit}>
          <Form.Input name="name" label="Nome" colSpan={6} />
          <Form.Input name="email" label="Email" type="email" colSpan={6} />
          <Form.Select 
            name="role" 
            label="Função" 
            options={[
              { value: 'admin', label: 'Admin' },
              { value: 'user', label: 'Usuário' }
            ]}
            colSpan={12}
          />
          <Form.Actions>
            <Form.Submit>Salvar</Form.Submit>
          </Form.Actions>
        </Form>
      )}
    </div>
  );
}
```
