<?xml version="1.0" encoding="utf-8"?>

<!-- filtro para Wix Harvest tool para excluir archivos de salida -->
<xsl:stylesheet version="1.0" 
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:wix="http://schemas.microsoft.com/wix/2006/wi"
                xmlns="http://schemas.microsoft.com/wix/2006/wi"
                exclude-result-prefixes="xsl wix">

  <xsl:output method="xml" indent="yes" omit-xml-declaration="yes" />

  <xsl:strip-space elements="*"/>

  <!-- Filtro para buscar elementos xml que contengan un .exe en el nombre de archivo -->
  <xsl:key name="ExeToRemove"
           match="wix:Component[contains(wix:File/@Source, '.exe')]"
           use="@Id" />
  <!-- Filtro para buscar elementos xml que contengan un .exe en el nombre de archivo -->
  <xsl:key name="ReadmeToRemove"
           match="wix:Component[contains(wix:File/@Source, '\Docs\Readme.rtf')]"
           use="@Id" />
  
  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

  <!-- Eliminar archivos .exe (seran añadidos manualmente por el instalador para uasr su version en el instalador) -->
  <xsl:template match="*[self::wix:Component or self::wix:ComponentRef]
                        [key('ExeToRemove', @Id)]" />
  <!-- Eliminar archivo readme.rtf (sera añadido manualmente por el instalador para poder abrirlo tras la instalacion) -->
  <xsl:template match="*[self::wix:Component or self::wix:ComponentRef]
                        [key('ReadmeToRemove', @Id)]" />
</xsl:stylesheet>