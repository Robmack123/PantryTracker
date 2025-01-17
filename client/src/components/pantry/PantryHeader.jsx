import { Row, Col, Input, Button } from "reactstrap";
import { CategoryDropdown } from "./CategoryFilter";

export const PantryHeader = ({
  selectedCategories,
  onCategorySelect,
  searchQuery,
  onSearchChange,
  onClearSearch,
  toggleAddModal,
}) => (
  <div className="sticky-filter bg-light p-3 mb-3">
    <Row className="align-items-center">
      <Col xs={12} md={4}>
        <CategoryDropdown
          selectedCategories={selectedCategories}
          onCategorySelect={onCategorySelect}
        />
      </Col>
      <Col xs={12} md={6} className="mt-2 mt-md-0">
        <div className="d-flex align-items-center">
          <Input
            type="text"
            placeholder="Search items..."
            value={searchQuery}
            onChange={(e) => onSearchChange(e.target.value)}
            style={{ flex: 1 }}
          />
          <Button
            color="secondary"
            size="sm"
            onClick={onClearSearch}
            className="ms-2"
          >
            Clear
          </Button>
        </div>
      </Col>
      <Col xs={12} md={2} className="mt-2 mt-md-0 text-end">
        <Button color="primary" size="sm" onClick={toggleAddModal}>
          Add New Item
        </Button>
      </Col>
    </Row>
  </div>
);
