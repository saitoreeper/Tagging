using Innovation.Horizons.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Innovation.Horizons.Helpers
{

    public class TagHelper
    {
        private static InnovationEngineDataContext db = new InnovationEngineDataContext();

        /// <summary>
        /// Splits a string by comma, this string shouldnt be null
        /// </summary>
        /// <param name="tags">comma seperated string</param>
        /// <returns>an array of type string</returns>
        public static string[] SplitTags(string tags)
        {
            // split string by comma
            var results = tags.Split(',');

            // remove all empty or null strings
            return results.Where(x => !string.IsNullOrWhiteSpace(x) && !string.IsNullOrEmpty(x)).ToArray();
        }

        /// <summary>
        /// Determins if a specific tag exists in the Tag table.
        /// </summary>
        /// <param name="tag">string to check, this should be pre-checked for whitespace or blank</param>
        /// <returns>True if the tag exists, false if does not exist</returns>
        public static tblTag TagExists(string tag)
        {
            var tagSearch = db.tblTags.Where(x => x.TagName == tag);

            return tagSearch.SingleOrDefault();


        }

        /// <summary>
        /// Add tag to tag table and set count to zero
        /// </summary>
        /// <param name="tagName">Tag to be added to the database, this should be pre-checked for whitespace or blank</param>
        /// <returns>Returns a true or false</returns>
        public static tblTag AddTag(string tagName)
        {
            var tag = new tblTag
            {
                TagName = tagName.Trim().ToLower(), // standerdise tags to lower case and trimmed
                Count = 0 // new tags is autommatically set to zero
            };

            db.tblTags.InsertOnSubmit(tag);
            db.SubmitChanges();
            return tag;
        }

        /// <summary>
        /// Increases the tag count by 1 for the specific tag
        /// </summary>
        /// <param name="TagID">ID of the tag to use</param>
        /// <returns>Return current counts of tags else null if no tag was found</returns>
        public static int? IncreaseTagCount(int tagID)
        {
            var tag = db.tblTags.Where(x => x.TagID == tagID);
            if (tag.SingleOrDefault() != null)
            {
                tag.SingleOrDefault().Count += 1;
                db.SubmitChanges();
                return Convert.ToInt32(tag.SingleOrDefault().Count);
            }
            return null;
        }

        /// <summary>
        /// Decrease the tag count by 1 for the specific tag
        /// </summary>
        /// <param name="TagID">ID of the tag to use</param>
        /// <returns>Return current counts of tags else null if no tag was found</returns>
        public static int? DecreaseTagCount(int tagID)
        {
            var tag = db.tblTags.Where(x => x.TagID == tagID);
            if (tag.SingleOrDefault() != null || tag.SingleOrDefault().Count >= 0 )
            {
                tag.SingleOrDefault().Count -= 1;
                db.SubmitChanges();
                return Convert.ToInt32(tag.SingleOrDefault().Count);
            }
            return null;
        }

        /// <summary>
        /// Associates specified tag to the specified Innovation
        /// </summary>
        /// <param name="tagID">ID of the tag to use</param>
        /// <param name="innovationID">ID of the innovation to use</param>
        /// <returns>Returns newly created tblTagInnovation else returns null </returns>
        public static tblTagInnovation AddTagAssociation(int tagID, int innovationID)
        {
            var tagInnovation = new tblTagInnovation
            {
                TagID = tagID,
                InnovationID = innovationID
            };
            db.tblTagInnovations.InsertOnSubmit(tagInnovation);
            db.SubmitChanges();
            return tagInnovation;
        }

        /// <summary>
        /// Remove the association between the specified tag and the specified Innovation
        /// </summary>
        /// <param name="tagID">ID of the tag to use</param>
        /// <param name="innovationID">ID of the innovation to use</param>
        /// <returns>Returns a true if association is succesfully removed</returns>
        public static tblTagInnovation RemoveTagAssociation( int innovationID)
        {
            var tagsToDelete = db.tblTagInnovations.Where(t => t.InnovationID == innovationID);
            
            if (tagsToDelete.Count() == 0)
            {
                return tagsToDelete.SingleOrDefault();
            }
            db.tblTagInnovations.DeleteAllOnSubmit(tagsToDelete);
            foreach (var tag in tagsToDelete)
            {
                TagHelper.DecreaseTagCount(tag.TagID);
            }
            
            db.SubmitChanges();
            return tagsToDelete.SingleOrDefault();
        }

        public static bool CreateTags(string tagList, int innovationID)
        {

            var tags = TagHelper.SplitTags(tagList);

            foreach (var _tag in tags)
            {

                var tagExists = TagHelper.TagExists(_tag);
                if (tagExists != null)
                {

                    TagHelper.IncreaseTagCount(tagExists.TagID);
                }
                else
                {
                    var tag = TagHelper.AddTag(_tag);
                    TagHelper.IncreaseTagCount(tag.TagID);
                }
                var tagAddID = db.tblTags.Where(x => x.TagName == _tag).FirstOrDefault().TagID;
                TagHelper.AddTagAssociation(tagAddID, innovationID);
            }
            return false;
        }

        public static bool EditTags(string tagList, tblInnovation innovation)
        {
            
            var tag = TagHelper.RemoveTagAssociation(innovation.InnovationID);
           
            
            TagHelper.CreateTags(tagList, innovation.InnovationID);
            return true;
        }

        //public static string CreateTags(tblInnovation innovation)
        //{
        //    string[] tagList = innovation.Tags.Split(',');



            //    foreach (var _tag in tagList)
            //    {
            //        var tag = new tblTag();
            //        var tagInnovation = new tblTagInnovation();

            //        var tagSearch = db.tblTags.Where(x => x.TagName == _tag);

            //        if (tagSearch.Count() == 0)
            //        {
            //            tag.TagName = _tag;
            //            tag.Count = 1;
            //            db.tblTags.InsertOnSubmit(tag);
            //            db.SubmitChanges();
            //        }
            //        else
            //        {
            //            tag.TagID = tagSearch.FirstOrDefault().TagID;
            //            tagSearch.FirstOrDefault().Count += 1;
            //        }

            //        //Add TagArticles
            //        tagInnovation.TagID = tag.TagID;
            //        tagInnovation.InnovationID = innovation.InnovationID;
            //        db.tblTagInnovations.InsertOnSubmit(tagInnovation);
            //        db.SubmitChanges();
            //    }

            //    return "Added Tags";
            //}

            //public static string EditTags(tblInnovation innovation)
            //{
            //    var tagsToDelete = db.tblTagInnovations.Where(t => t.InnovationID == innovation.InnovationID);
            //    var tagFind = db.tblTagInnovations.Where(t => t.InnovationID == innovation.InnovationID);
            //    db.tblTagInnovations.DeleteAllOnSubmit(tagsToDelete);



            //    foreach (var tag in tagFind)
            //    {
            //        var tagSearch = db.tblTags.Where(x => x.TagName == tag.tblTag.TagName);
            //        tagSearch.FirstOrDefault().Count -= 1;
            //        if (!string.IsNullOrWhiteSpace(tag.tblTag.TagName))
            //        {
            //            db.SubmitChanges();
            //        }

            //    }







            //    TagHelper.CreateTags(innovation);


            //    return "tags added";
            //}



        }
}