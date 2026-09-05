import type {
  OrganizationalUnitDto,
  EmployeeDto,
  CostCenterDto,
  ProjectDto,
  CreateOrgUnitCommand,
  UpdateOrgUnitCommand,
  CreateEmployeeCommand,
  UpdateEmployeeCommand,
  CreateCostCenterCommand,
  UpdateCostCenterCommand,
  CreateProjectCommand,
  UpdateProjectCommand,
} from './types';

const BASE_ORG_UNITS = '/api/OrganizationalUnits';
const BASE_EMPLOYEES = '/api/Employees';
const BASE_COST_CENTERS = '/api/CostCenters';
const BASE_PROJECTS = '/api/Projects';

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const text = await response.text();
    throw new Error(text || `HTTP ${response.status}`);
  }
  return response.json();
}

async function handleVoid(response: Response): Promise<void> {
  if (!response.ok) {
    const text = await response.text();
    throw new Error(text || `HTTP ${response.status}`);
  }
}

// Organizational Units
export const orgUnitsClient = {
  async list(): Promise<OrganizationalUnitDto[]> {
    return handleResponse<OrganizationalUnitDto[]>(
      await fetch(BASE_ORG_UNITS, { headers: { Accept: 'application/json' } })
    );
  },
  async getById(id: number): Promise<OrganizationalUnitDto> {
    return handleResponse<OrganizationalUnitDto>(
      await fetch(`${BASE_ORG_UNITS}/${id}`, { headers: { Accept: 'application/json' } })
    );
  },
  async create(data: CreateOrgUnitCommand): Promise<void> {
    await handleVoid(
      await fetch(BASE_ORG_UNITS, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        body: JSON.stringify(data),
      })
    );
  },
  async update(data: UpdateOrgUnitCommand): Promise<void> {
    await handleVoid(
      await fetch(`${BASE_ORG_UNITS}/${data.id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        body: JSON.stringify(data),
      })
    );
  },
  async delete(id: number): Promise<void> {
    await handleVoid(
      await fetch(`${BASE_ORG_UNITS}/${id}`, {
        method: 'DELETE',
        headers: { Accept: 'application/json' },
      })
    );
  },
};

// Employees
export const employeesClient = {
  async list(): Promise<EmployeeDto[]> {
    return handleResponse<EmployeeDto[]>(
      await fetch(BASE_EMPLOYEES, { headers: { Accept: 'application/json' } })
    );
  },
  async getById(id: number): Promise<EmployeeDto> {
    return handleResponse<EmployeeDto>(
      await fetch(`${BASE_EMPLOYEES}/${id}`, { headers: { Accept: 'application/json' } })
    );
  },
  async create(data: CreateEmployeeCommand): Promise<void> {
    await handleVoid(
      await fetch(BASE_EMPLOYEES, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        body: JSON.stringify(data),
      })
    );
  },
  async update(data: UpdateEmployeeCommand): Promise<void> {
    await handleVoid(
      await fetch(`${BASE_EMPLOYEES}/${data.id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        body: JSON.stringify(data),
      })
    );
  },
  async delete(id: number): Promise<void> {
    await handleVoid(
      await fetch(`${BASE_EMPLOYEES}/${id}`, {
        method: 'DELETE',
        headers: { Accept: 'application/json' },
      })
    );
  },
};

// Cost Centers
export const costCentersClient = {
  async list(): Promise<CostCenterDto[]> {
    return handleResponse<CostCenterDto[]>(
      await fetch(BASE_COST_CENTERS, { headers: { Accept: 'application/json' } })
    );
  },
  async getById(id: number): Promise<CostCenterDto> {
    return handleResponse<CostCenterDto>(
      await fetch(`${BASE_COST_CENTERS}/${id}`, { headers: { Accept: 'application/json' } })
    );
  },
  async create(data: CreateCostCenterCommand): Promise<void> {
    await handleVoid(
      await fetch(BASE_COST_CENTERS, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        body: JSON.stringify(data),
      })
    );
  },
  async update(data: UpdateCostCenterCommand): Promise<void> {
    await handleVoid(
      await fetch(`${BASE_COST_CENTERS}/${data.id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        body: JSON.stringify(data),
      })
    );
  },
  async delete(id: number): Promise<void> {
    await handleVoid(
      await fetch(`${BASE_COST_CENTERS}/${id}`, {
        method: 'DELETE',
        headers: { Accept: 'application/json' },
      })
    );
  },
};

// Projects
export const projectsClient = {
  async list(): Promise<ProjectDto[]> {
    return handleResponse<ProjectDto[]>(
      await fetch(BASE_PROJECTS, { headers: { Accept: 'application/json' } })
    );
  },
  async getById(id: number): Promise<ProjectDto> {
    return handleResponse<ProjectDto>(
      await fetch(`${BASE_PROJECTS}/${id}`, { headers: { Accept: 'application/json' } })
    );
  },
  async create(data: CreateProjectCommand): Promise<void> {
    await handleVoid(
      await fetch(BASE_PROJECTS, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        body: JSON.stringify(data),
      })
    );
  },
  async update(data: UpdateProjectCommand): Promise<void> {
    await handleVoid(
      await fetch(`${BASE_PROJECTS}/${data.id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        body: JSON.stringify(data),
      })
    );
  },
  async delete(id: number): Promise<void> {
    await handleVoid(
      await fetch(`${BASE_PROJECTS}/${id}`, {
        method: 'DELETE',
        headers: { Accept: 'application/json' },
      })
    );
  },
};
