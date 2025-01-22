import { useState, useEffect } from "react";
import {
  Modal,
  ModalHeader,
  ModalBody,
  Button,
  Input,
  FormGroup,
  Table,
} from "reactstrap";
import { searchBrandedFood } from "../../managers/pantryItemManager";

export const SearchPantryItemModal = ({ isOpen, toggle, onSelectItem }) => {
  const [searchQuery, setSearchQuery] = useState("");
  const [searchResults, setSearchResults] = useState([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (!isOpen) {
      setSearchQuery("");
      setSearchResults([]);
    }
  }, [isOpen]);

  const handleSearch = () => {
    if (!searchQuery.trim()) return; // Ignore empty searches
    setLoading(true);
    searchBrandedFood(searchQuery)
      .then((results) => {
        setSearchResults(results.items || []);
        setLoading(false);
      })
      .catch((err) => {
        console.error("Error searching branded items:", err);
        setLoading(false);
      });
  };

  return (
    <Modal isOpen={isOpen} toggle={toggle} size="lg" centered scrollable>
      <ModalHeader toggle={toggle}>Search Pantry Items</ModalHeader>
      <ModalBody>
        <FormGroup>
          <Input
            type="text"
            placeholder="Search for an item (e.g., milk)"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
          <Button
            color="primary"
            className="mt-2 w-100"
            onClick={handleSearch}
            disabled={!searchQuery.trim() || loading}
          >
            {loading ? "Searching..." : "Search"}
          </Button>
        </FormGroup>

        {searchResults.length > 0 && (
          <Table striped bordered hover responsive className="mt-3">
            <thead>
              <tr>
                <th>Name</th>
                <th>Brand</th>
                <th>Action</th>
              </tr>
            </thead>
            <tbody>
              {searchResults.map((item) => (
                <tr key={item.barcode}>
                  <td>{item.name}</td>
                  <td>{item.brand || "Unknown Brand"}</td>
                  <td>
                    <Button
                      color="success"
                      size="sm"
                      onClick={() => onSelectItem(item)}
                    >
                      Select
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </Table>
        )}
      </ModalBody>
    </Modal>
  );
};
