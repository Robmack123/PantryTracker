import { useState, useEffect } from "react";
import {
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Button,
  Form,
  FormGroup,
  Label,
  Input,
} from "reactstrap";
import { addOrUpdatePantryItem } from "../../managers/pantryItemManager";
import { CategoryDropdown } from "./CategoryFilter";
import { SearchPantryItemModal } from "./SearchPantryItemModal";

export const AddPantryItemModal = ({ isOpen, toggle, refreshPantryItems }) => {
  const [itemName, setItemName] = useState("");
  const [quantity, setQuantity] = useState(1);
  const [selectedCategories, setSelectedCategories] = useState([]);
  const [monitorLowStock, setMonitorLowStock] = useState(true);
  const [error, setError] = useState("");
  const [searchModalOpen, setSearchModalOpen] = useState(false);

  useEffect(() => {
    if (!isOpen) {
      setItemName("");
      setQuantity(1);
      setSelectedCategories([]);
      setMonitorLowStock(true);
      setError("");
    }
  }, [isOpen]);

  const handleSubmit = (e) => {
    e.preventDefault();
    setError("");

    if (selectedCategories.length === 0) {
      setError("Please select at least one category.");
      return;
    }

    const newItem = {
      name: itemName,
      quantity: parseInt(quantity, 10),
      categoryIds: selectedCategories,
      monitorLowStock,
    };

    addOrUpdatePantryItem(newItem)
      .then(() => {
        refreshPantryItems();
        toggle();
      })
      .catch((err) => {
        console.error("Error adding pantry item:", err);
        setError("Failed to add pantry item.");
      });
  };

  const openSearchModal = () => setSearchModalOpen(true);
  const closeSearchModal = () => setSearchModalOpen(false);

  return (
    <>
      <Modal isOpen={isOpen} toggle={toggle} centered scrollable size="lg">
        <ModalHeader toggle={toggle}>Add New Pantry Item</ModalHeader>
        <ModalBody>
          {error && <p className="text-danger">{error}</p>}
          <Form onSubmit={handleSubmit}>
            <Button
              color="secondary"
              className="mb-3"
              onClick={openSearchModal}
            >
              Search Pantry Items
            </Button>
            <FormGroup>
              <Label for="itemName">Item Name</Label>
              <Input
                type="text"
                id="itemName"
                value={itemName}
                onChange={(e) => setItemName(e.target.value)}
                required
              />
            </FormGroup>
            <FormGroup>
              <Label for="quantity">Quantity</Label>
              <Input
                type="number"
                id="quantity"
                value={quantity}
                onChange={(e) => setQuantity(e.target.value)}
                min="1"
                required
              />
            </FormGroup>
            <FormGroup>
              <Label>Categories</Label>
              <CategoryDropdown
                onCategorySelect={setSelectedCategories}
                selectedCategories={selectedCategories}
              />
              {selectedCategories.length === 0 && (
                <p className="text-danger mt-2">
                  Please select at least one category.
                </p>
              )}
            </FormGroup>
            <FormGroup>
              <Label for="monitorLowStock">Monitor Low Stock</Label>
              <Input
                type="checkbox"
                id="monitorLowStock"
                checked={monitorLowStock}
                onChange={(e) => setMonitorLowStock(e.target.checked)}
              />
            </FormGroup>
          </Form>
        </ModalBody>
        <ModalFooter>
          <Button color="primary" onClick={handleSubmit}>
            Add Item
          </Button>
          <Button color="secondary" onClick={toggle}>
            Cancel
          </Button>
        </ModalFooter>
      </Modal>

      <SearchPantryItemModal
        isOpen={searchModalOpen}
        toggle={closeSearchModal}
        onSelectItem={(item) => {
          setItemName(item.name);
          setSearchModalOpen(false);
        }}
      />
    </>
  );
};
