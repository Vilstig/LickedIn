import { test, expect } from '@playwright/test';

test.use({
  storageState: 'auth.json'
});

test('test', async ({ page }) => {
  await page.goto('http://localhost:5176/');
  await page.getByRole('link', { name: 'Zarządzaj Projektami' }).click();
  await page.getByRole('link', { name: ' Utwórz nowy projekt' }).click();
  await page.getByRole('textbox', { name: 'Nazwa Projektu' }).click();
  await page.getByRole('textbox', { name: 'Nazwa Projektu' }).fill('asdasd');
  await page.getByLabel('Kierownik').selectOption('1');
  await page.locator('select[name="TeamMembers[0].RequiredSkills[0].SkillTypeId"]').selectOption('1');
  await page.getByRole('textbox', { name: 'np. Senior Backend Dev' }).click();
  await page.getByRole('textbox', { name: 'np. Senior Backend Dev' }).fill('dasd');
  await page.getByRole('link', { name: 'Anuluj' }).click();
});