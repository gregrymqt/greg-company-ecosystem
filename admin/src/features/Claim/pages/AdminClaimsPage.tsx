import React, { useState, useEffect, useCallback } from "react";
import styles from "./ClaimsLayout.module.scss";
import { ClaimChat, ClaimsList } from "@/features/claim";
import { AdminClaimService } from "@/features/claim";
import { Modal } from "@/components/Modal/Modal";
import type { SidebarItem } from "@/components/SideBar/types/sidebar.types";
import { Sidebar } from "@/components/SideBar";
import type { ClaimSummary } from "../types/claims.types";

// Menu da Sidebar do Admin
const MENU_ITEMS: SidebarItem[] = [
  { id: "opened", label: "Caixa de Entrada", icon: "fas fa-inbox" },
  { id: "mediation", label: "Em Mediação (MP)", icon: "fas fa-gavel" },
  { id: "closed", label: "Resolvidos / Histórico", icon: "fas fa-history" },
];

export const AdminClaimsPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState("opened");
  const [claims, setClaims] = useState<ClaimSummary[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  // Estado para controlar qual disputa está aberta no Modal
  const [selectedClaimId, setSelectedClaimId] = useState<string | null>(null);

  const loadClaims = useCallback(async () => {
    setIsLoading(true);
    try {
      // Mapeia a aba visual para o filtro da API
      const statusFilter =
        activeTab === "closed"
          ? "closed"
          : activeTab === "mediation"
          ? "mediation"
          : "opened";

      const response = await AdminClaimService.getAll(currentPage, "", statusFilter);
      setClaims(response.claims);
      setTotalPages(response.totalPages);
    } catch (error) {
      console.error(error);
    } finally {
      setIsLoading(false);
    }
  }, [activeTab, currentPage]);

  // Busca dados quando a função loadClaims mudar (que depende de activeTab)
  useEffect(() => {
    loadClaims();
  }, [loadClaims]);

  return (
    <div className={styles.layoutContainer}>
      <Sidebar
        items={MENU_ITEMS}
        activeItemId={activeTab}
        onItemClick={(item) => {
          setActiveTab(String(item.id));
          setCurrentPage(1);
        }}
        logo={<h3 className="p-4 font-bold text-white">Admin Painel</h3>}
      >
        <div className="p-4 text-xs text-white opacity-70">
          Greg Company © 2024
        </div>
      </Sidebar>

      <main className={styles.mainContent}>
        <h1 className={styles.pageTitle}>Gerenciamento de Disputas</h1>

        <ClaimsList
          data={claims}
          isLoading={isLoading}
          userRole="user"
          onViewDetails={(claim: ClaimSummary) => setSelectedClaimId(claim.internalId)}
        />

        <div className="flex justify-center items-center gap-4 mt-6 mb-8">
          <button 
            disabled={currentPage === 1} 
            onClick={() => setCurrentPage(prev => prev - 1)}
            className="px-4 py-2 bg-gray-200 text-gray-700 rounded disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-300 transition-colors"
          >
            Anterior
          </button>
          <span className="text-sm font-medium text-gray-600">
            Página {currentPage} de {totalPages}
          </span>
          <button 
            disabled={currentPage === totalPages} 
            onClick={() => setCurrentPage(prev => prev + 1)}
            className="px-4 py-2 bg-gray-200 text-gray-700 rounded disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-300 transition-colors"
          >
            Próxima
          </button>
        </div>
      </main>

      <Modal
        isOpen={!!selectedClaimId}
        onClose={() => {
          setSelectedClaimId(null);
          loadClaims();
        }}
        title="Detalhes e Chat"
        size="large"
      >
        {selectedClaimId && (
          <ClaimChat claimId={selectedClaimId}  />
        )}
      </Modal>
    </div>
  );
};
