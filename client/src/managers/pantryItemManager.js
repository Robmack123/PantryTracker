const apiUrl =
  "https://pantrytrackingapp-degqcdguf7dbg0c4.canadacentral-01.azurewebsites.net/api/pantryitem";

// Helper function to get auth headers
const getAuthHeaders = () => {
  const token = localStorage.getItem("authToken");
  return token ? { Authorization: `Bearer ${token}` } : {};
};

export const getPantryItems = (page = 1, pageSize = 10, searchQuery = "") => {
  const query = `page=${page}&pageSize=${pageSize}${
    searchQuery ? `&searchQuery=${encodeURIComponent(searchQuery)}` : ""
  }`;
  return fetch(`${apiUrl}?${query}`, {
    method: "GET",
    headers: {
      "Content-Type": "application/json",
      ...getAuthHeaders(),
    },
  }).then((res) => {
    if (!res.ok) {
      throw new Error("Failed to fetch pantry items.");
    }
    return res.json();
  });
};

export const getPantryItemsByCategory = (
  categoryIds,
  page = 1,
  pageSize = 10
) => {
  const queryString = categoryIds.map((id) => `categoryIds=${id}`).join("&");
  return fetch(
    `${apiUrl}/by-category?${queryString}&page=${page}&pageSize=${pageSize}`,
    {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        ...getAuthHeaders(),
      },
    }
  ).then((res) => {
    if (!res.ok) {
      throw new Error("Failed to fetch pantry items by category.");
    }
    return res.json();
  });
};

export const addOrUpdatePantryItem = (pantryItem) => {
  return fetch(`${apiUrl}`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      ...getAuthHeaders(),
    },
    body: JSON.stringify(pantryItem),
  }).then((res) => {
    if (!res.ok) {
      throw new Error("Failed to add or update pantry item.");
    }
    return res.json();
  });
};

export const updatePantryItemQuantity = (itemId, dto) => {
  return fetch(`${apiUrl}/${itemId}/quantity`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
      ...getAuthHeaders(),
    },
    body: JSON.stringify(dto),
  }).then((res) => {
    if (!res.ok) {
      throw new Error("Failed to update quantity.");
    }
    return res.json();
  });
};

export const deletePantryItem = (id) => {
  return fetch(`${apiUrl}/${id}`, {
    method: "DELETE",
    headers: {
      "Content-Type": "application/json",
      ...getAuthHeaders(),
    },
  }).then((res) => {
    if (!res.ok) {
      throw new Error("Failed to delete pantry item.");
    }
    return res.json();
  });
};

export const toggleMonitorLowStock = (id) => {
  return fetch(`${apiUrl}/${id}/toggle-monitor`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
      ...getAuthHeaders(),
    },
  }).then((res) => {
    if (!res.ok) {
      throw new Error("Failed to toggle MonitorLowStock.");
    }
    return res.json();
  });
};

export const searchBrandedFood = (name, limit = 10, page = 1) => {
  return fetch(
    `/api/pantryitem/search-branded?name=${encodeURIComponent(
      name
    )}&limit=${limit}&page=${page}`,
    {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        ...getAuthHeaders(),
      },
    }
  ).then((res) => {
    if (!res.ok) {
      throw new Error("Failed to fetch branded food items.");
    }
    return res.json();
  });
};

export const updatePantryItemDetails = (id, dto) => {
  return fetch(`/api/pantryitem/${id}`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
      ...getAuthHeaders(),
    },
    body: JSON.stringify(dto),
  }).then((res) => {
    if (!res.ok) {
      throw new Error("Failed to update pantry item.");
    }
    return res.json();
  });
};
