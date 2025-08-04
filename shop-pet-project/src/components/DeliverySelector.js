import { useEffect, useState } from "react";
import { API_BASE_URL } from "../apiConfig";

const deliveryUrl = `${API_BASE_URL}orders-api/api/delivery-locations`;
const selectStyle = "form-select bg-secondary text-white border-0";

const DeliverySelector = ({ onSelectionChange }) => {
  const [regions, setRegions] = useState([]);
  const [cities, setCities] = useState([]);
  const [warehouses, setWarehouses] = useState([]);

  const [selectedRegion, setSelectedRegion] = useState(null);
  const [selectedCity, setSelectedCity] = useState(null);
  const [selectedWarehouse, setSelectedWarehouse] = useState(null);

  useEffect(() => {
    const fetchRegions = async () => {
      const res = await fetch(`${deliveryUrl}/regions`);
      const data = await res.json();
      setRegions(data);
    };
    fetchRegions();
  }, []);

  useEffect(() => {
    if (selectedRegion) {
      setSelectedCity(null);
      setSelectedWarehouse(null);
      setCities([]);
      setWarehouses([]);

      fetch(`${deliveryUrl}/cities/${selectedRegion.ref}`)
        .then(res => res.json())
        .then(data => setCities(data));
    }
  }, [selectedRegion]);

  useEffect(() => {
    if (selectedCity) {
      setSelectedWarehouse(null);
      setWarehouses([]);

      fetch(`${deliveryUrl}/warehouses/${selectedCity.ref}`)
        .then(res => res.json())
        .then(data => setWarehouses(data));
    }
  }, [selectedCity]);

  useEffect(() => {
    onSelectionChange({
      region: selectedRegion,
      city: selectedCity,
      warehouse: selectedWarehouse
    });
  }, [selectedRegion, selectedCity, selectedWarehouse]);

  return (
    <div className="row mb-3">
      <div className="col-md-4">
        <label className="form-label text-light">Region</label>
        <select
          className={selectStyle}
          value={selectedRegion?.ref || ""}
          onChange={(e) => {
            const region = regions.find(r => r.ref === e.target.value);
            setSelectedRegion(region || null);
          }}
        >
          <option value="">Select region</option>
          {regions.map(r => (
            <option key={r.ref} value={r.ref}>{r.description}</option>
          ))}
        </select>
      </div>
      <div className="col-md-4">
        <label className="form-label text-light">City</label>
        <select
          className={selectStyle}
          value={selectedCity?.ref || ""}
          onChange={(e) => {
            const city = cities.find(c => c.ref === e.target.value);
            setSelectedCity(city || null);
          }}
          disabled={!cities.length}
        >
          <option value="">Select city</option>
          {cities.map(c => (
            <option key={c.ref} value={c.ref}>{c.description}</option>
          ))}
        </select>
      </div>
      <div className="col-md-4">
        <label className="form-label text-light">Warehouse</label>
        <select
          className={selectStyle}
          value={selectedWarehouse?.ref || ""}
          onChange={(e) => {
            const warehouse = warehouses.find(w => w.ref === e.target.value);
            setSelectedWarehouse(warehouse || null);
          }}
          disabled={!warehouses.length}
        >
          <option value="">Select warehouse</option>
          {warehouses.map(w => (
            <option key={w.ref} value={w.ref}>{w.description}</option>
          ))}
        </select>
      </div>
    </div>
  );
};

export default DeliverySelector;