import { useEffect, useState } from "react"
import { API_BASE_URL } from "../../apiConfig";
import ProductCard from "../../components/ProductCard";
import ProductFilterPanel from "../../components/ProductFilterPanel";

const productsUrl = `${API_BASE_URL}products-api/api/products`;

const Products = () => {
    const [filters, setFilters] = useState({});
    const [pageParams, setPageParams] = useState({ page: 1, pageSize: 12 });
    const [paginationData, setPaginationData] = useState({
        items: [],
        page: 1,
        pageSize: 12,
        totalCount: 0,
        hasNextPage: false,
        hasPreviousPage: false
    });

    useEffect(() => {
        fetchProducts();
    }, [filters, pageParams]);

    const fetchProducts = async () => {
        const params = new URLSearchParams();

        if (filters.name) params.append("Name", filters.name);
        if (filters.price) params.append("Price", filters.price);
        if (filters.isAvailable != null) params.append("IsAvailable", filters.isAvailable);
        if (filters.rating != null) params.append("Rating", filters.rating);
        filters.categories?.forEach((c) => params.append("Categories", c));
        if (filters.sortParams?.orderBy) params.append("OrderBy", filters.sortParams.orderBy);
        if (filters.sortParams?.sortDirection) params.append("SortDirection", filters.sortParams.sortDirection);
        params.append("FilterMode", filters.filterMode);
        params.append("Page", pageParams.page);
        params.append("PageSize", pageParams.pageSize);

        const response = await fetch(`${productsUrl}?${params.toString()}`, { credentials: 'include' });
        const data = await response.json();
        setPaginationData(data);
    };

    const handlePageChange = (newPage) => {
        setPageParams((prev) => ({ ...prev, page: newPage }));
    };

    const totalPages = Math.ceil(paginationData.totalCount / paginationData.pageSize);

    return (
        <div className="container mt-4">
            <div className="row">
                <div className="col-md-3">
                    <ProductFilterPanel onFilterChange={setFilters} />
                </div>
                <div className="col-md-9">
                    <div className="row">
                        {paginationData?.items?.length > 0 ? (
                            paginationData.items.map((product) => (
                                <ProductCard key={product.id} product={product} />
                            ))
                            ) : (
                            <p className="text-light">No products found.</p>
                        )}
                    </div>

                    <div className="d-flex justify-content-between align-items-center mt-4 text-light">
                        <button
                            className="btn btn-outline-light"
                            disabled={!paginationData.hasPreviousPage}
                            onClick={() => handlePageChange(paginationData.page - 1)}
                        >
                            ← Previous
                        </button>

                        <span>Page {paginationData.page} of {totalPages}</span>

                        <button
                            className="btn btn-outline-light"
                            disabled={!paginationData.hasNextPage}
                            onClick={() => handlePageChange(paginationData.page + 1)}
                        >
                            Next →
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Products;