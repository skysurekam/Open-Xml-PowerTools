﻿/***************************************************************************

Copyright (c) Microsoft Corporation 2012-2015.

This code is licensed using the Microsoft Public License (Ms-PL).  The text of the license can be found here:

http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx

Published at http://OpenXmlDeveloper.org
Resource Center and Documentation: http://openxmldeveloper.org/wiki/w/wiki/powertools-for-open-xml.aspx

Developer: Eric White
Blog: http://www.ericwhite.com
Twitter: @EricWhiteDev
Email: eric@ericwhite.com

***************************************************************************/

#define COPY_FILES_FOR_DEBUGGING

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;
using OpenXmlPowerTools;
using Xunit;

namespace OxPt
{
    public class CaTests
    {
        [Theory]
        [InlineData("CA001-Plain.docx", 60)]
        [InlineData("CA002-Bookmark.docx", 7)]
        [InlineData("CA003-Numbered-List.docx", 8)]
        [InlineData("CA004-TwoParas.docx", 88)]
        [InlineData("CA005-Table.docx", 27)]
        [InlineData("CA006-ContentControl.docx", 60)]
        [InlineData("CA007-DayLong.docx", 10)]
        [InlineData("CA008-Footnote-Reference.docx", 23)]
        [InlineData("CA010-Delete-Run.docx", 16)]
        [InlineData("CA011-Insert-Run.docx", 16)]
        [InlineData("CA012-fldSimple.docx", 10)]
        [InlineData("CA013-Lots-of-Stuff.docx", 168)]
        [InlineData("CA014-Complex-Table.docx", 193)]
        [InlineData("WC024-Table-Before.docx", 24)]
        [InlineData("WC024-Table-After2.docx", 18)]
        //[InlineData("", 0)]
        //[InlineData("", 0)]
        //[InlineData("", 0)]
        //[InlineData("", 0)]
        
        public void CA001_ContentAtoms(string name, int contentAtomCount)
        {
            FileInfo sourceDocx = new FileInfo(Path.Combine(TestUtil.SourceDir.FullName, name));

            var thisGuid = Guid.NewGuid().ToString().Replace("-", "");
            var sourceCopiedToDestDocx = new FileInfo(Path.Combine(TestUtil.TempDir.FullName, sourceDocx.Name.Replace(".docx", string.Format("-{0}-1-Source.docx", thisGuid))));
            if (!sourceCopiedToDestDocx.Exists)
                File.Copy(sourceDocx.FullName, sourceCopiedToDestDocx.FullName);

            var coalescedDocx = new FileInfo(Path.Combine(TestUtil.TempDir.FullName, sourceDocx.Name.Replace(".docx", string.Format("-{0}-2-Coalesced.docx", thisGuid))));
            if (!coalescedDocx.Exists)
                File.Copy(sourceDocx.FullName, coalescedDocx.FullName);

            var contentAtomDataFi = new FileInfo(Path.Combine(TestUtil.TempDir.FullName, sourceDocx.Name.Replace(".docx", string.Format("-{0}-3-ContentAtomData.txt", thisGuid))));

            using (WordprocessingDocument wDoc = WordprocessingDocument.Open(coalescedDocx.FullName, true))
            {
                var contentParent = wDoc.MainDocumentPart.GetXDocument().Root.Element(W.body);
                ComparisonUnitAtom[] contentAtomList = WmlComparer.CreateComparisonUnitAtomList(wDoc.MainDocumentPart, contentParent);
                StringBuilder sb = new StringBuilder();
                var part = wDoc.MainDocumentPart;

                sb.AppendFormat("Part: {0}", part.Uri.ToString());
                sb.Append(Environment.NewLine);
                sb.Append(ComparisonUnit.ComparisonUnitListToString(contentAtomList.ToArray()) + Environment.NewLine);
                sb.Append(Environment.NewLine);

                XDocument newMainXDoc = WmlComparer.Coalesce(contentAtomList);
                var partXDoc = wDoc.MainDocumentPart.GetXDocument();
                partXDoc.Root.ReplaceWith(newMainXDoc.Root);
                wDoc.MainDocumentPart.PutXDocument();

                File.WriteAllText(contentAtomDataFi.FullName, sb.ToString());

                Assert.Equal(contentAtomCount, contentAtomList.Count());
            }
        }

        [Theory]
        [InlineData("HC009-Test-04.docx")]
        public void CA002_Annotations(string name)
        {
            FileInfo sourceDocx = new FileInfo(Path.Combine(TestUtil.SourceDir.FullName, name));

#if COPY_FILES_FOR_DEBUGGING
            var sourceCopiedToDestDocx = new FileInfo(Path.Combine(TestUtil.TempDir.FullName, sourceDocx.Name.Replace(".docx", "-1-Source.docx")));
            if (!sourceCopiedToDestDocx.Exists)
                File.Copy(sourceDocx.FullName, sourceCopiedToDestDocx.FullName);

            var annotatedDocx = new FileInfo(Path.Combine(TestUtil.TempDir.FullName, sourceDocx.Name.Replace(".docx", "-2-Annotated.docx")));
            if (!annotatedDocx.Exists)
                File.Copy(sourceDocx.FullName, annotatedDocx.FullName);

            using (WordprocessingDocument wDoc = WordprocessingDocument.Open(annotatedDocx.FullName, true))
            {
                var contentParent = wDoc.MainDocumentPart.GetXDocument().Root.Element(W.body);
                WmlComparer.CreateComparisonUnitAtomList(wDoc.MainDocumentPart, contentParent);
            }
#endif
        }

        [Theory]
        [InlineData("CA009-altChunk.docx")]
        //[InlineData("")]
        //[InlineData("")]
        //[InlineData("")]

        public void CA003_ContentAtoms_Throws(string name)
        {
            FileInfo sourceDocx = new FileInfo(Path.Combine(TestUtil.SourceDir.FullName, name));
            var thisGuid = Guid.NewGuid().ToString().Replace("-", "");
            var sourceCopiedToDestDocx = new FileInfo(Path.Combine(TestUtil.TempDir.FullName, sourceDocx.Name.Replace(".docx", string.Format("-{0}-1-Source.docx", thisGuid))));
            if (!sourceCopiedToDestDocx.Exists)
                File.Copy(sourceDocx.FullName, sourceCopiedToDestDocx.FullName);

            var coalescedDocx = new FileInfo(Path.Combine(TestUtil.TempDir.FullName, sourceDocx.Name.Replace(".docx", string.Format("-{0}-2-Coalesced.docx", thisGuid))));
            if (!coalescedDocx.Exists)
                File.Copy(sourceDocx.FullName, coalescedDocx.FullName);

            var contentAtomDataFi = new FileInfo(Path.Combine(TestUtil.TempDir.FullName, sourceDocx.Name.Replace(".docx", string.Format("-{0}-3-ContentAtomData.txt", thisGuid))));

            using (WordprocessingDocument wDoc = WordprocessingDocument.Open(coalescedDocx.FullName, true))
            {
                Assert.Throws<NotSupportedException>(() =>
                {
                    var contentParent = wDoc.MainDocumentPart.GetXDocument().Root.Element(W.body);
                    WmlComparer.CreateComparisonUnitAtomList(wDoc.MainDocumentPart, contentParent);
                });
            }
        }
    }
}