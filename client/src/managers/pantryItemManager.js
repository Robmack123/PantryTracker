const apiUrl = "/api/pantryitem";

export const getPantryItems = (page = 1, pageSize = 10, searchQuery = "") => {
  const query = `page=${page}&pageSize=${pageSize}${
    searchQuery ? `&searchQuery=${encodeURIComponent(searchQuery)}` : ""
  }`;
  return fetch(`${apiUrl}?${query}`, {
    method: "GET",
    credentials: "include",
    headers: {
      "Content-Type": "application/json",
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
      credentials: "include",
      headers: {
        "Content-Type": "application/json",
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
    credentials: "include",
    headers: {
      "Content-Type": "application/json",
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
    credentials: "include",
    headers: {
      "Content-Type": "application/json",
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
    credentials: "include",
    headers: {
      "Content-Type": "application/json",
    },
  }).then((res) => {
    if (!res.ok) {
      throw new Error("Failed to toggle MonitorLowStock.");
    }
    return res.json();
  });
};
