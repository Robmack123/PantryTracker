import { useEffect, useState } from "react";
import {
  getPantryItems,
  getPantryItemsByCategory,
} from "../../managers/pantryItemManager";
import { Card, CardBody, CardTitle, Container, Alert } from "reactstrap";
import { PantryHeader } from "./PantryHeader";
import { PantryTable } from "./PantryTable";
import { PaginationControls } from "./PaginationControls";
import { AddPantryItemModal } from "./AddPantryItemModal";
import { SearchPantryItemModal } from "./SearchPantryItemModal";
import { ProductDetailsModal } from "./ProductDetailsModa";
import "./pantryList.css";

export const PantryItems = () => {
  const [pantryItems, setPantryItems] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [addModalOpen, setAddModalOpen] = useState(false);
  const [searchModalOpen, setSearchModalOpen] = useState(false);
  const [detailsOpen, setDetailsOpen] = useState(false);
  const [selectedProduct, setSelectedProduct] = useState(null);
  const [selectedCategories, setSelectedCategories] = useState([]);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalItems, setTotalItems] = useState(0);
  const [searchQuery, setSearchQuery] = useState("");
  const itemsPerPage = 9;

  useEffect(() => {
    fetchPantryItems(selectedCategories, currentPage, searchQuery);
  }, [currentPage, selectedCategories, searchQuery]);

  const fetchPantryItems = (categoryIds = [], page = 1, query = "") => {
    setLoading(true);
    setError("");

    const fetchFunction = categoryIds.length
      ? () => getPantryItemsByCategory(categoryIds, page, itemsPerPage, query)
      : () => getPantryItems(page, itemsPerPage, query);

    fetchFunction()
      .then((data) => {
        const items = Array.isArray(data) ? data : data.items || [];
        setPantryItems(items);
        setTotalItems(data.totalItems || items.length);
        setLoading(false);
      })
      .catch((err) => {
        console.error("Error fetching pantry items:", err);
        setError("Failed to load pantry items.");
        setLoading(false);
      });
  };

  const toggleAddModal = () => setAddModalOpen(!addModalOpen);
  const toggleSearchModal = () => setSearchModalOpen(!searchModalOpen);

  const openDetailsModal = (product) => {
    setSelectedProduct(product);
    setDetailsOpen(true);
  };

  const closeDetailsModal = () => {
    setSelectedProduct(null);
    setDetailsOpen(false);
  };

  const handleItemSelect = (item) => {
    console.log("Selected item:", item);
    toggleSearchModal();
  };

  return (
    <div className="fullscreen-background">
      <Container className="d-flex justify-content-center align-items-center">
        <Card outline color="primary" className="card-centered shadow-lg">
          <CardBody>
            <CardTitle tag="h3" className="text-center">
              Pantry Items
            </CardTitle>
            <PantryHeader
              selectedCategories={selectedCategories}
              onCategorySelect={setSelectedCategories}
              searchQuery={searchQuery}
              onSearchChange={setSearchQuery}
              onClearSearch={() => setSearchQuery("")}
              toggleAddModal={toggleAddModal}
              toggleSearchModal={toggleSearchModal}
            />
            {loading && <p>Loading pantry items...</p>}
            {error && <Alert color="danger">{error}</Alert>}
            {!loading && (
              <>
                <PantryTable
                  pantryItems={pantryItems}
                  currentPage={currentPage}
                  itemsPerPage={itemsPerPage}
                  onRowClick={openDetailsModal}
                />
                <PaginationControls
                  currentPage={currentPage}
                  totalItems={totalItems}
                  itemsPerPage={itemsPerPage}
                  onPageChange={setCurrentPage}
                />
              </>
            )}
          </CardBody>
        </Card>
      </Container>
      <AddPantryItemModal
        isOpen={addModalOpen}
        toggle={toggleAddModal}
        refreshPantryItems={() =>
          fetchPantryItems(selectedCategories, currentPage, searchQuery)
        }
      />
      <SearchPantryItemModal
        isOpen={searchModalOpen}
        toggle={toggleSearchModal}
        onSelectItem={handleItemSelect}
      />
      {selectedProduct && (
        <ProductDetailsModal
          isOpen={detailsOpen}
          toggle={closeDetailsModal}
          product={selectedProduct}
          refreshPantryItems={() =>
            fetchPantryItems(selectedCategories, currentPage, searchQuery)
          }
        />
      )}
    </div>
  );
};
