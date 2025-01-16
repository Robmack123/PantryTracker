import { useEffect, useState } from "react";
import {
  getPantryItems,
  getPantryItemsByCategory,
} from "../../managers/pantryItemManager";
import {
  Card,
  CardBody,
  CardTitle,
  Table,
  Alert,
  Button,
  Input,
} from "reactstrap";
import { CategoryDropdown } from "./CategoryFilter";
import { AddPantryItemModal } from "./AddPantryItemModal";
import { ProductDetailsModal } from "./ProductDetailsModa";
import "./pantryList.css";

export const PantryItems = () => {
  const [pantryItems, setPantryItems] = useState([]);
  const [filteredItems, setFilteredItems] = useState([]); // For search results
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [modalOpen, setModalOpen] = useState(false);
  const [detailsOpen, setDetailsOpen] = useState(false);
  const [selectedProduct, setSelectedProduct] = useState(null);
  const [selectedCategories, setSelectedCategories] = useState([]);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalItems, setTotalItems] = useState(0); // Total items in the list
  const [searchQuery, setSearchQuery] = useState(""); // Search input state
  const itemsPerPage = 10;

  useEffect(() => {
    fetchPantryItems(selectedCategories, 1, searchQuery); // Reset to page 1 when search query changes
    setCurrentPage(1);
  }, [searchQuery]);

  useEffect(() => {
    // Filter items whenever searchQuery changes
    const filtered = pantryItems.filter((item) =>
      item.name.toLowerCase().includes(searchQuery.toLowerCase())
    );
    setFilteredItems(filtered);
  }, [searchQuery, pantryItems]);

  const fetchPantryItems = (
    categoryIds = selectedCategories,
    page = currentPage,
    query = searchQuery // Include the search query
  ) => {
    setLoading(true);
    setError("");

    const fetchFunction = categoryIds.length
      ? () => getPantryItemsByCategory(categoryIds, page, itemsPerPage)
      : () => getPantryItems(page, itemsPerPage, query); // Pass the search query

    fetchFunction()
      .then((data) => {
        setPantryItems(data.items || []);
        setTotalItems(data.totalItems || 0);
        setLoading(false);
      })
      .catch((err) => {
        console.error("Error fetching pantry items:", err);
        setError("Failed to load pantry items.");
        setLoading(false);
      });
  };

  const handleCategorySelect = (categoryIds) => {
    setSelectedCategories(categoryIds);
    setCurrentPage(1);
    fetchPantryItems(categoryIds, 1);
  };

  const toggleModal = () => setModalOpen(!modalOpen);

  const openDetailsModal = (product) => {
    setSelectedProduct(product);
    setDetailsOpen(true);
  };

  const closeDetailsModal = () => {
    setSelectedProduct(null);
    setDetailsOpen(false);
  };

  return (
    <div>
      <Card className="mt-4" outline color="primary">
        <CardBody>
          <CardTitle tag="h3">Pantry Items</CardTitle>
          <div className="d-flex align-items-center mb-3">
            <CategoryDropdown
              onCategorySelect={handleCategorySelect}
              selectedCategories={selectedCategories}
            />
            <div>
              <Input
                type="text"
                placeholder="Search items..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="ms-3"
                style={{ maxWidth: "300px" }}
              />
            </div>
            <Button
              color="primary"
              size="sm"
              onClick={toggleModal}
              className="ms-3"
            >
              Add New Item
            </Button>
          </div>
          {loading && <p>Loading pantry items...</p>}
          {error && (
            <Alert color="danger" timeout={3000}>
              {error}
            </Alert>
          )}
          {filteredItems.length > 0 ? (
            <>
              <Table
                bordered
                hover
                className="table-light table-hover w-100 custom-table"
                style={{ fontSize: "1.2rem" }}
              >
                <thead>
                  <tr>
                    <th>#</th>
                    <th>Name</th>
                    <th>Quantity</th>
                    <th>Last Updated</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredItems.map((item, index) => (
                    <tr
                      key={item.id}
                      onClick={() => openDetailsModal(item)}
                      style={{ cursor: "pointer" }}
                    >
                      <td>{index + 1 + (currentPage - 1) * itemsPerPage}</td>
                      <td>{item.name}</td>
                      <td>{item.quantity}</td>
                      <td>{new Date(item.updatedAt).toLocaleString()}</td>
                    </tr>
                  ))}
                </tbody>
              </Table>

              <div className="d-flex justify-content-between align-items-center mt-3">
                <Button
                  color="secondary"
                  size="sm"
                  onClick={() =>
                    setCurrentPage((prev) => Math.max(prev - 1, 1))
                  }
                  disabled={currentPage === 1}
                >
                  Previous
                </Button>
                <span>
                  Page {currentPage} of {Math.ceil(totalItems / itemsPerPage)}
                </span>
                <Button
                  color="secondary"
                  size="sm"
                  onClick={() =>
                    setCurrentPage((prev) =>
                      Math.min(prev + 1, Math.ceil(totalItems / itemsPerPage))
                    )
                  }
                  disabled={currentPage >= Math.ceil(totalItems / itemsPerPage)}
                >
                  Next
                </Button>
              </div>
            </>
          ) : (
            !loading && <p>No items found.</p>
          )}
        </CardBody>
      </Card>
      <AddPantryItemModal
        isOpen={modalOpen}
        toggle={toggleModal}
        refreshPantryItems={() =>
          fetchPantryItems(selectedCategories, currentPage)
        }
      />
      {selectedProduct && (
        <ProductDetailsModal
          isOpen={detailsOpen}
          toggle={closeDetailsModal}
          product={selectedProduct}
          refreshPantryItems={() =>
            fetchPantryItems(selectedCategories, currentPage)
          }
        />
      )}
    </div>
  );
};
