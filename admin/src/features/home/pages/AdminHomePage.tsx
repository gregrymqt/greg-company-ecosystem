import React, { useState } from "react";
import styles from "./styles/AdminHomePage.module.scss"; // Vamos criar/reutilizar este estilo

import { Sidebar } from "@/components/SideBar";
import type { SidebarItem } from "@/components/SideBar/types/sidebar.types";
import {
  HeroForm,
  HeroList,
  ServiceForm,
  ServiceList,
  useAdminHomeData,
  useAdminHero,
  useAdminServices
} from "@/features/home";
import type { HeroSlideDto, ServiceDto, HeroFormValues, ServiceFormValues } from "@/features/home/types/home.types";

// --- CONFIGURAÇÃO DA SIDEBAR ---
const sidebarItems: SidebarItem[] = [
  { id: "forms", label: "Gerenciar Cadastros", icon: "fas fa-pen-square" },
  { id: "list", label: "Visualizar Dados", icon: "fas fa-table" },
];

export const AdminHomePage: React.FC = () => {
  // --- ESTADOS DE NAVEGAÇÃO ---
  const [activeSidebarId, setActiveSidebarId] = useState<string | number>(
    "list"
  );
  const [activeTab, setActiveTab] = useState<"hero" | "services">("hero");

  // --- ESTADOS DE DADOS (Leitura) ---
  // O hook useHomeData já retorna heroSlides e services separados
  const {
    heroSlides,
    services,
    isLoading: isFetching,
    refreshData,
  } = useAdminHomeData();

  // --- ESTADOS DE EDIÇÃO ---
  const [editingHero, setEditingHero] = useState<HeroSlideDto | undefined>(
    undefined
  );
  const [editingService, setEditingService] = useState<ServiceDto | undefined>(
    undefined
  );

  // --- HOOKS DE ESCRITA (CRUD) ---
  const {
    createHero,
    updateHero,
    deleteHero,
    isLoading: loadingHero,
  } = useAdminHero();

  const {
    createService,
    updateService,
    deleteService,
    isLoading: loadingService,
  } = useAdminServices();

  // --- HANDLERS DE EDIÇÃO (Vindos da Lista) ---
  const handleEditHero = (item: HeroSlideDto) => {
    setEditingHero(item);
    setActiveTab("hero");
    setActiveSidebarId("forms"); // Joga para a aba de formulário
  };

  const handleEditService = (item: ServiceDto) => {
    setEditingService(item);
    setActiveTab("services");
    setActiveSidebarId("forms");
  };

  // --- HANDLER DE SUCESSO (Após Create/Update) ---
  const handleSuccess = () => {
    refreshData(); // Recarrega a lista do servidor
    setEditingHero(undefined); // Limpa estados de edição
    setEditingService(undefined);
    setActiveSidebarId("list"); // Volta para a visualização
  };

  // --- HANDLER DE CANCELAMENTO ---
  const handleCancel = () => {
    setEditingHero(undefined);
    setEditingService(undefined);
    setActiveSidebarId("list");
  };

  // --- RENDERIZAÇÃO CONDICIONAL ---
  const renderContent = () => {
    const isFormsMode = activeSidebarId === "forms";

    // A. MODO FORMULÁRIO
    if (isFormsMode) {
      if (activeTab === "hero") {
        return (
          <HeroForm
            initialData={editingHero}
            onSubmit={(data: HeroFormValues) => {
              if (editingHero) {
                updateHero(editingHero.id, data, handleSuccess);
              } else {
                createHero(data, handleSuccess);
              }
            }}
            isLoading={loadingHero}
            onCancel={handleCancel}
          />
        );
      } else {
        return (
          <ServiceForm
            initialData={editingService}
            onSubmit={(data: ServiceFormValues) => {
              if (editingService) {
                updateService(editingService.id, data, handleSuccess);
              } else {
                createService(data, handleSuccess);
              }
            }}
            isLoading={loadingService}
            onCancel={handleCancel}
          />
        );
      }
    }

    // B. MODO LISTA
    else {
      if (activeTab === "hero") {
        return (
          <HeroList
            data={heroSlides}
            isLoading={isFetching}
            onEdit={handleEditHero}
            onDelete={(id: number) => deleteHero(id, refreshData)}
          />
        );
      } else {
        return (
          <ServiceList
            data={services}
            isLoading={isFetching}
            onEdit={handleEditService}
            onDelete={(id: number) => deleteService(id, refreshData)}
          />
        );
      }
    }
  };

  return (
    <div className={styles.pageContainer}>
      {/* SIDEBAR */}
      <Sidebar
        items={sidebarItems}
        activeItemId={activeSidebarId}
        onItemClick={(item: SidebarItem) => {
          setActiveSidebarId(item.id);
          // Ao trocar de menu principal, limpamos a edição para evitar confusão
          if (item.id === "list") {
            setEditingHero(undefined);
            setEditingService(undefined);
          }
        }}
        logo={<h3>Admin Home</h3>}
      >
        <div
          style={{
            padding: "1rem",
            borderTop: "1px solid #ddd",
            color: "#666",
          }}
        >
          <small>Módulo Home</small>
        </div>
      </Sidebar>

      {/* ÁREA DE CONTEÚDO */}
      <main className={styles.contentArea}>
        <header>
          <h1>
            {activeSidebarId === "forms" ? "Gerenciamento" : "Visualização"}
          </h1>
          <p>Configure os banners e serviços da página inicial.</p>
        </header>

        {/* ABAS INTERNAS */}
        <nav className={styles.tabsContainer}>
          <button
            className={`${styles.tabButton} ${
              activeTab === "hero" ? styles.active : ""
            }`}
            onClick={() => {
              setActiveTab("hero");
              setEditingHero(undefined);
              setEditingService(undefined);
            }}
          >
            <i className="fas fa-images" style={{ marginRight: 8 }}></i>
            Hero (Carrossel)
          </button>

          <button
            className={`${styles.tabButton} ${
              activeTab === "services" ? styles.active : ""
            }`}
            onClick={() => {
              setActiveTab("services");
              setEditingHero(undefined);
              setEditingService(undefined);
            }}
          >
            <i className="fas fa-concierge-bell" style={{ marginRight: 8 }}></i>
            Serviços
          </button>
        </nav>

        {/* CONTEÚDO DINÂMICO */}
        <div className="fade-in">{renderContent()}</div>
      </main>
    </div>
  );
};
