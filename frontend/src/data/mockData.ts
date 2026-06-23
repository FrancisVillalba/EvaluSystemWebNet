export type OrderStatus =
  | "Cargado"
  | "Diseno aprobado"
  | "Pendiente de impresion"
  | "En impresion"
  | "Impreso"
  | "Entregado"
  | "Pendiente de pago";

export type OrderDetail = {
  product: string;
  machine: string;
  quantity: string;
  unitPrice: string;
  extraPrice: string;
  designName: string;
};

export type Order = {
  id: string;
  date: string;
  client: string;
  seller: string;
  status: OrderStatus;
  delivery: string;
  paymentMethod: string;
  paymentStatus: string;
  paidAmount: string;
  proofName: string;
  notes: string;
  details: OrderDetail[];
};

export type Client = {
  id: number;
  name: string;
  document: string;
  phone: string;
  email: string;
  city: string;
  carrier: boolean;
  status: "Activo" | "Inactivo";
};

export const orders: Order[] = [
  {
    id: "DTF-1048",
    date: "2026-06-22",
    client: "Urban Print Co.",
    seller: "Camila",
    status: "Pendiente de impresion",
    delivery: "2026-06-22",
    paymentMethod: "Transferencia",
    paymentStatus: "Pendiente",
    paidAmount: "0",
    proofName: "",
    notes: "",
    details: [
      {
        product: "DTF Textil por metro",
        machine: "Epson DTF 60cm",
        quantity: "18",
        unitPrice: "80000",
        extraPrice: "",
        designName: "YESSICA MORINIGO TEXTIL.png"
      }
    ]
  },
  {
    id: "UV-1047",
    date: "2026-06-22",
    client: "Brava Store",
    seller: "Martin",
    status: "Impreso",
    delivery: "2026-06-22",
    paymentMethod: "Transferencia",
    paymentStatus: "Parcial",
    paidAmount: "400000",
    proofName: "",
    notes: "",
    details: [
      {
        product: "Sticker UV DTF",
        machine: "UV DTF A3+",
        quantity: "9",
        unitPrice: "95000",
        extraPrice: "",
        designName: "logo-brava.png"
      }
    ]
  },
  {
    id: "DTF-1046",
    date: "2026-06-22",
    client: "Norte Uniformes",
    seller: "Sofia",
    status: "Entregado",
    delivery: "2026-06-23",
    paymentMethod: "Efectivo",
    paymentStatus: "Pagado",
    paidAmount: "2120000",
    proofName: "",
    notes: "",
    details: [
      {
        product: "DTF Textil por metro",
        machine: "Epson DTF 60cm",
        quantity: "24",
        unitPrice: "82000",
        extraPrice: "152000",
        designName: "uniformes-norte.png"
      }
    ]
  },
  {
    id: "UV-1045",
    date: "2026-06-21",
    client: "Mateo Accesorios",
    seller: "Camila",
    status: "Cargado",
    delivery: "2026-06-22",
    paymentMethod: "Tarjeta",
    paymentStatus: "Pendiente",
    paidAmount: "0",
    proofName: "",
    notes: "",
    details: [
      {
        product: "Sticker UV DTF",
        machine: "UV DTF A3+",
        quantity: "6",
        unitPrice: "90000",
        extraPrice: "",
        designName: "sticker-mateo.png"
      }
    ]
  }
];

export const clients: Client[] = [
  {
    id: 1,
    name: "Urban Print Co.",
    document: "80012345-1",
    phone: "0981 220 440",
    email: "ventas@urbanprint.com",
    city: "Asuncion",
    carrier: false,
    status: "Activo"
  },
  {
    id: 2,
    name: "Brava Store",
    document: "80155220-7",
    phone: "0972 880 112",
    email: "pedidos@bravastore.com",
    city: "San Lorenzo",
    carrier: false,
    status: "Activo"
  },
  {
    id: 3,
    name: "Casa Grafica",
    document: "80045090-2",
    phone: "0984 610 722",
    email: "admin@casagrafica.com",
    city: "Luque",
    carrier: true,
    status: "Inactivo"
  }
];
