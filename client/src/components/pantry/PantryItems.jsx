import { useEffect, useState } from "react";
import {
  getPantryItems,
  getPantryItemsByCategory,
} from "../../managers/pantryItemManager";
import { Table, Alert, Button } from "reactstrap";
import { CategoryDropdown } from "./CategoryFilter";
import { AddPantryItemModal } from "./AddPantryItemModal";
import { ProductDetailsModal } from "./ProductDetailsModa";
import "./pantryList.css";

export const PantryItems = () => {
  const [pantryItems, setPantryItems] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [modalOpen, setModalOpen] = useState(false);
  const [detailsOpen, setDetailsOpen] = useState(false); // State to track details modal
  const [selectedProduct, setSelectedProduct] = useState(null); // State to track the selected product
  const [selectedCategories, setSelectedCategories] = useState([]); // Track selected categories

  useEffect(() => {
    fetchPantryItems();
  }, []);

  const fetchPantryItems = (categoryIds = []) => {
    setLoading(true);
    setError("");

    const fetchFunction = categoryIds.length
      ? () => getPantryItemsByCategory(categoryIds)
      : getPantryItems;

    fetchFunction()
      .then((items) => {
        setPantryItems(items);
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
    fetchPantryItems(categoryIds);
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
      {/* <Card className="mt-4">
        <CardBody>
          <CardTitle tag="h3">Pantry Items</CardTitle> */}
      <div className="d-flex align-items-center mb-3">
        {/* Category Dropdown */}
        <CategoryDropdown
          onCategorySelect={handleCategorySelect}
          selectedCategories={selectedCategories}
        />
        <Button
          color="primary"
          size="sm"
          onClick={toggleModal}
          className="ms-3"
        >
          Add New Item
        </Button>
      </div>

      {/* Loading, Error, and Pantry Table */}
      {loading && <p>Loading pantry items...</p>}
      {error && (
        <Alert color="danger" timeout={3000}>
          {error}
        </Alert>
      )}
      {pantryItems.length > 0 ? (
        <Table
          bordered
          hover
          className="table-light table-hover w-100 custom-table"
          style={{ fontSize: "1.2rem" }} // Larger text for readability
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
            {pantryItems.map((item, index) => (
              <tr
                key={item.id}
                onClick={() => openDetailsModal(item)}
                style={{ cursor: "pointer" }}
              >
                <td>{index + 1}</td>
                <td>{item.name}</td>
                <td>{item.quantity}</td>
                <td>{new Date(item.updatedAt).toLocaleString()}</td>
              </tr>
            ))}
          </tbody>
        </Table>
      ) : (
        !loading && <p>No items found for the selected category.</p>
      )}
      {/* </CardBody>
      </Card> */}

      {/* Add Pantry Item Modal */}
      <AddPantryItemModal
        isOpen={modalOpen}
        toggle={toggleModal}
        refreshPantryItems={() => fetchPantryItems(selectedCategories)}
      />

      {/* Product Details Modal */}
      {selectedProduct && (
        <ProductDetailsModal
          isOpen={detailsOpen}
          toggle={closeDetailsModal}
          product={selectedProduct}
          refreshPantryItems={() => fetchPantryItems(selectedCategories)}
        />
      )}
    </div>
  );
};
