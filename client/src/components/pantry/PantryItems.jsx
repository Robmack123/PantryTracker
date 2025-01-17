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
  Container,
  Row,
  Col,
} from "reactstrap";
import { CategoryDropdown } from "./CategoryFilter";
import { AddPantryItemModal } from "./AddPantryItemModal";
import { ProductDetailsModal } from "./ProductDetailsModa";
import "./pantryList.css";

export const PantryItems = () => {
  const [pantryItems, setPantryItems] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [modalOpen, setModalOpen] = useState(false);
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

  const handleCategorySelect = (categoryIds) => {
    setSelectedCategories(categoryIds);
    setCurrentPage(1);
    fetchPantryItems(categoryIds, 1, searchQuery);
  };

  const handleSearchChange = (e) => {
    setSearchQuery(e.target.value);
    setCurrentPage(1);
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

  const handleClearSearch = () => {
    setSearchQuery("");
    setCurrentPage(1);
    fetchPantryItems(selectedCategories, 1, "");
  };

  return (
    <div className="fullscreen-background">
      <Container className="d-flex justify-content-center align-items-center">
        <Card outline color="primary" className="card-centered shadow-lg">
          <CardBody>
            <CardTitle tag="h3" className="text-center">
              Pantry Items
            </CardTitle>
            <div className="sticky-filter bg-light p-3 mb-3">
              <Row className="align-items-center">
                <Col xs={12} md={4}>
                  <CategoryDropdown
                    onCategorySelect={handleCategorySelect}
                    selectedCategories={selectedCategories}
                  />
                </Col>
                <Col xs={12} md={6} className="mt-2 mt-md-0">
                  <div className="d-flex align-items-center">
                    <Input
                      type="text"
                      placeholder="Search items..."
                      value={searchQuery}
                      onChange={handleSearchChange}
                      style={{ flex: 1 }}
                    />
                    <Button
                      color="secondary"
                      size="sm"
                      onClick={handleClearSearch}
                      className="ms-2"
                    >
                      Clear
                    </Button>
                  </div>
                </Col>
                <Col xs={12} md={2} className="mt-2 mt-md-0 text-end">
                  <Button color="primary" size="sm" onClick={toggleModal}>
                    Add New Item
                  </Button>
                </Col>
              </Row>
            </div>
            {loading && <p>Loading pantry items...</p>}
            {error && (
              <Alert color="danger" timeout={3000}>
                {error}
              </Alert>
            )}
            {pantryItems.length > 0 ? (
              <>
                <div className="table-container">
                  <Table
                    bordered
                    hover
                    className="table-light table-hover custom-table"
                  >
                    <thead className="table-primary">
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
                          style={{
                            cursor: "pointer",
                            backgroundColor:
                              item.quantity < 2 && item.monitorLowStock
                                ? "#fff6f6"
                                : "inherit",
                          }}
                        >
                          <td>
                            {index + 1 + (currentPage - 1) * itemsPerPage}
                          </td>
                          <td>{item.name}</td>
                          <td>
                            {item.quantity}{" "}
                            {item.quantity < 4 && item.monitorLowStock && (
                              <span className="badge bg-danger ms-1">Low</span>
                            )}
                          </td>
                          <td>{new Date(item.updatedAt).toLocaleString()}</td>
                        </tr>
                      ))}
                    </tbody>
                  </Table>
                </div>
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
                    disabled={
                      currentPage >= Math.ceil(totalItems / itemsPerPage)
                    }
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
      </Container>
      <AddPantryItemModal
        isOpen={modalOpen}
        toggle={toggleModal}
        refreshPantryItems={() =>
          fetchPantryItems(selectedCategories, currentPage, searchQuery)
        }
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
